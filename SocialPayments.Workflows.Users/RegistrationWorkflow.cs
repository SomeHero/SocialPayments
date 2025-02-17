﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using SocialPayments.Domain;
using NLog;
using System.Text.RegularExpressions;
using System.Data.Entity;

namespace SocialPayments.Workflows.Users
{
    public class RegistrationWorkflow
    {
        private readonly Context _ctx = new Context();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        DomainServices.EmailService emailService = null;
        DomainServices.ApplicationService applicationServices = null;
        DomainServices.SMSLogService smsLogServices = null;
        DomainServices.SMSService smsService = null;
        DomainServices.TransactionBatchService transactionBatchService = null;

        public RegistrationWorkflow()
        {
            emailService = new DomainServices.EmailService(_ctx);
            applicationServices = new DomainServices.ApplicationService();
            smsLogServices = new SMSLogService(_ctx);
            smsService = new DomainServices.SMSService(applicationServices, smsLogServices, _ctx, logger);
            transactionBatchService = new DomainServices.TransactionBatchService(_ctx, logger);
        }
        /// <summary>
        /// Process New PaidThx Member Registration
        /// </summary>
        /// <param name="id">unique identified of the user</param>
        public void ProcessNewUserRegistration(string id)
        {
            String message = "";

            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Starting.", id));

            Guid userId;
            Guid.TryParse(id, out userId);

            if (userId == null)
            {
                logger.Log(LogLevel.Error, string.Format("Error Processing New User Registration for {0}", id));
                throw new Exception(string.Format("Error Processing New User Registration for {0}", id));
            }

            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Retrieving User.", userId));
            var user = _ctx.Users
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                logger.Log(LogLevel.Error, string.Format("Error Processing New User Registration for {0}. Unable to find User.", userId));
            }

            switch (user.UserStatus)
            {
                case UserStatus.Submitted:
                    //if valid phone # and phone # not verified
                    if (user.MobileNumber.Length > 0)
                    {
                        logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Sending Verification Codes.", userId));

                        //get random alpha numerics
                        user.MobileVerificationCode1 = "1234";
                        user.MobileVerificationCode2 = "4321";

                        message = string.Format("Welcome to PdThx.   Your verfication codes are {0} and {1}.", user.MobileVerificationCode1, user.MobileVerificationCode2);
 
                        //sms mobile verification codes
                        smsService.SendSMS(user.ApiKey, user.MobileNumber, message);
                    }

                    user.UserStatus = UserStatus.Pending;

                    _ctx.SaveChanges();
                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Finished.", id));

                    break;
                case UserStatus.Pending:

                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Retrieving Associated Payments.", userId));

                    if (user.PaymentAccounts.Count == 0)
                    {
                        logger.Log(LogLevel.Error, string.Format("No payment accounts found for user {0}", user.UserId));
                    }
                    else
                    {
                        var paymentAccount = user.PaymentAccounts[0];
                        TransactionBatch transactionBatch = null;

                        try
                        {
                            transactionBatch = _ctx.TransactionBatches.FirstOrDefault(t => t.IsClosed == false) ??
                                _ctx.TransactionBatches.Add(new TransactionBatch());
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, string.Format("Exceptioon retreiving current batch file. {0}", ex.Message));
                        }
                        
                        var payments = _ctx.Messages
                            .Include("Transactions")
                            .Include("FromAccount")
                            .Include("FromAccount.User")
                            .Where(p => p.RecipientUri == user.MobileNumber && p.Transactions.Count < 2); //  && p.PaymentStatus == PaymentStatus.Pending);

                        foreach (var payment in payments)
                        {
                            logger.Log(LogLevel.Info, string.Format("Processing User Registration for {0}, Creating Deposit for Payment {1}", userId, payment.Id));

                            payment.SenderAccountId = paymentAccount.Id;

                            try
                            {
                                payment.Transactions.Add(new Transaction()
                                {
                                    Amount = payment.Amount,
                                    Category = TransactionCategory.Payment,
                                    CreateDate = System.DateTime.Now,
                                    FromAccount = paymentAccount,
                                    PaymentChannelType = PaymentChannelType.Single,
                                    //PaymentId = payment.Id,
                                    Id = Guid.NewGuid(),
                                    StandardEntryClass = StandardEntryClass.Web,
                                    Status = TransactionStatus.Pending,
                                    TransactionBatch = transactionBatch,
                                    Type = TransactionType.Deposit
                                });
                            }
                            catch (Exception ex)
                            {
                                logger.Log(LogLevel.Error, string.Format("Exception creating deposit for Payment {0}. {1}", payment.Id, ex.Message));
                                logger.Log(LogLevel.Error, string.Format("Inner Exception creating deposit for Payment {0}. {1}", payment.Id, ex.InnerException.Message));

                                throw ex;
                            }

                            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending SMS to Payee at {1} f0r Payment {2}", userId, user.MobileNumber, payment.Id));

                            message = string.Format("A payment in the amounnt of {0:C} from {1} was successfully completed.  {0:C} will be deposited into your bank account.", payment.Amount, payment.SenderUri);

                            smsService.SendSMS(payment.ApiKey, user.MobileNumber, message);


                            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending Confirmation Email to Payee at {1} for Payment {2}", userId, user.MobileNumber, payment.Id));

                            //emailService.SendEmail(new Services.DataContracts.Email.EmailRequest()
                            //{
                            //    ApiKey = payment.ApiKey,
                            //    FromAddress = "admin@pdthx.me",
                            //    Body = string.Format("A payment in the amount of {0:C} from {1} was successfully completed.  {0:C} will be deposited into your bank account.", payment.PaymentAmount, payment.FromMobileNumber),
                            //    EmailLogId = Guid.NewGuid(),
                            //    Subject = string.Format("You received {0} from {1}", payment.PaymentAmount, payment.FromMobileNumber),
                            //    ToAddress = user.EmailAddress
                            //});

                            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending SMS to Payer at {1} for Payment {2}", userId, user.MobileNumber, payment.Id));

                            message = string.Format("Your payment in the amounnt of {0} to {1} was successfully completed.  {0} will be deposited into the recipient's bank account.", payment.Amount, payment.SenderUri);

                            smsService.SendSMS(payment.ApiKey, payment.SenderUri, message);

                            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending Confirmation Email to Payer at {1} for Payment {2}", userId, user.MobileNumber, payment.Id));

                            //emailService.SendEmail(new Services.DataContracts.Email.EmailRequest()
                            //{
                            //    ApiKey = payment.ApiKey,
                            //    FromAddress = "admin@pdthx.me",
                            //    Body = string.Format("Your payment in the amount of {0:C} from {1} was successfully completed.  {0:C} will be deposited into the recipient's bank account.", payment.PaymentAmount, payment.FromMobileNumber),
                            //    EmailLogId = Guid.NewGuid(),
                            //    Subject = string.Format("Your payment of {0} to {1} is complete.", payment.PaymentAmount, payment.FromMobileNumber),
                            //    ToAddress = payment.FromAccount.User.EmailAddress,
                            //});
                        }
                    }

                    try
                    {
                        logger.Log(LogLevel.Info, String.Format("Saving Changes."));
                        _ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, String.Format("Exception Saving Changes. {0}", ex.Message));

                        var innerException = ex.InnerException;
                        while (innerException != null)
                        {
                            logger.Log(LogLevel.Error, string.Format("Inner Exception Saving Changes. {0}", ex.InnerException.Message));
                            innerException = innerException.InnerException;
                        }

                    }

                    break;
            }
        }
    }
}
