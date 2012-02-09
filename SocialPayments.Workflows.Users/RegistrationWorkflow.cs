using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using SocialPayments.Services;
using SocialPayments.Domain;
using NLog;
using System.Text.RegularExpressions;

namespace SocialPayments.Workflows.Users
{
    public class RegistrationWorkflow
    {
        private readonly Context _ctx = new Context();
        EmailService emailService = new EmailService();
        SMSService smsService = new SMSService();
        TransactionBatchService transactionBatchService = new TransactionBatchService();

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public void ProcessNewUserRegistration(string id)
        {
            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Starting.", id));

            Guid userId;
            Guid.TryParse(id, out userId);

            if(userId == null)
            {
                logger.Log(LogLevel.Error, string.Format("Error Processing New User Registration for {0}", id));
                throw new Exception(string.Format("Error Processing New User Registration for {0}", id));
            }
            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Retrieving User.", userId));
            var user = _ctx.Users.FirstOrDefault(u => u.UserId == userId);

            if(user == null) {
                logger.Log(LogLevel.Error, string.Format("Error Processing New User Registration for {0}. Unable to find User.", userId));
            }
            
            if (user.RegistrationMethod == UserRegistrationMethod.MobilePhone)
            {
                logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Sending Verification Codes.", userId));
                //get random alpha numerics
                user.MobileVerificationCode1 = "1234";
                user.MobileVerificationCode2 = "4321";

                //sms mobile verification codes
                smsService.SendSMS(new Services.DataContracts.SMS.SMSRequest()
                {
                    ApiKey = user.ApiKey,
                    Message = string.Format("Welcome to PdThx.   Your verfication codes are {0} and {1}.", user.MobileVerificationCode1, user.MobileVerificationCode2),
                    MobileNumber = user.MobileNumber,
                    SMSMessageId = Guid.NewGuid()
                });
            }
            else
            {
                logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Retrieving Associated Payments.", userId));
                var payments = _ctx.Payments.Where(p => p.ToMobileNumber == user.MobileNumber && p.Transactions.Count < 2); //  && p.PaymentStatus == PaymentStatus.Pending);

                foreach (var payment in payments)
                {
                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Creating Deposit for Payment {1}", userId, payment.Id));

                    var transactionBatch = transactionBatchService.GetOpenBatch();

                    payment.Transactions.Add(new Transaction()
                    {
                        Amount = payment.PaymentAmount,
                        Category = TransactionCategory.Payment,
                        CreateDate = System.DateTime.Now,
                        FromAccount = user.PaymentAccounts[0],
                        PaymentChannelType = PaymentChannelType.Single,
                        PaymentId = payment.Id,
                        Id = Guid.NewGuid(),
                        StandardEntryClass = StandardEntryClass.Web,
                        Status = TransactionStatus.Pending,
                        TransactionBatch = transactionBatch,
                        Type = TransactionType.Deposit
                    });

                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending SMS to Payee at {1} f0r Payment {2}", userId, user.MobileNumber, payment.Id));

                    smsService.SendSMS(new Services.DataContracts.SMS.SMSRequest()
                    {
                        ApiKey = payment.ApiKey,
                        Message = string.Format("A payment in the amounnt of {0} from {1} was successfully completed.  {0} will be deposited into your bank account.", payment.PaymentAmount, payment.FromMobileNumber),
                        MobileNumber = user.MobileNumber,
                        SMSMessageId = Guid.NewGuid()
                    });

                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending Confirmation Email to Payee at {1} f0r Payment {2}", userId, user.MobileNumber, payment.Id));

                    emailService.SendEmail(new Services.DataContracts.Email.EmailRequest()
                    {
                        ApiKey = payment.ApiKey,
                        FromAddress = "admin@pdthx.me",
                        Body = string.Format("A payment in the amounnt of {0} from {1} was successfully completed.  {0} will be deposited into your bank account.", payment.PaymentAmount, payment.FromMobileNumber),
                        EmailLogId = Guid.NewGuid(),
                        Subject = string.Format("You received {0} from {1}", payment.PaymentAmount, payment.FromMobileNumber),
                        ToAddress = user.EmailAddress
                    });

                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending SMS to Payer at {1} f0r Payment {2}", userId, user.MobileNumber, payment.Id));

                    smsService.SendSMS(new Services.DataContracts.SMS.SMSRequest()
                    {
                        ApiKey = payment.ApiKey,
                        Message = string.Format("Your payment in the amounnt of {0} to {1} was successfully completed.  {0} will be deposited into the recipient's bank account.", payment.PaymentAmount, payment.FromMobileNumber),
                        MobileNumber = payment.FromMobileNumber,
                        SMSMessageId = Guid.NewGuid()
                    });

                    logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Sending Confirmation Email to Payer at {1} f0r Payment {2}", userId, user.MobileNumber, payment.Id));

                    emailService.SendEmail(new Services.DataContracts.Email.EmailRequest()
                    {
                        ApiKey = payment.ApiKey,
                        FromAddress = "admin@pdthx.me",
                        Body = string.Format("Your payment in the amounnt of {0} from {1} was successfully completed.  {0} will be deposited into the recipient's bank account.", payment.PaymentAmount, payment.FromMobileNumber),
                        EmailLogId = Guid.NewGuid(),
                        Subject = string.Format("Your payment of {0} to {1} is complete.", payment.PaymentAmount, payment.FromMobileNumber),
                        ToAddress = payment.FromAccount.User.EmailAddress,
                    });
                }
            }
            _ctx.SaveChanges();
            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Finished.", id));
        }
    }
}
