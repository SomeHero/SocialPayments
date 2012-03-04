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
using System.Configuration;

namespace SocialPayments.Workflows.Payments
{
    public class PaymentWorkflow
    {
        private readonly Context _ctx = new Context();
        EmailService emailService = new EmailService();
        SMSService smsService = new SMSService();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Process user Initiated Payment to another user
        /// </summary>
        /// <param name="paymentId">the unique id of the payment</param>
        public void ProcessPayment(string paymentId)
        {
            var payment = _ctx.Payments.FirstOrDefault(p => p.Id == new Guid(paymentId));

            if(payment == null)
                throw new Exception("Payment id is invalid");

            logger.Log(LogLevel.Info, String.Format("Process Payment {0}", payment.Id.ToString()));

            switch (payment.PaymentStatus)
            {
                case PaymentStatus.Submitted:
                    string fromAddress = ConfigurationManager.AppSettings["FromAddress"];

                    string phoneNumberUnformatted = Regex.Replace(payment.ToMobileNumber, @"\D", string.Empty);

                    if (phoneNumberUnformatted.Length != 10)
                        throw new Exception(String.Format("To Mobile Number is not valid {0}", phoneNumberUnformatted));
                    
                    string areaCode = phoneNumberUnformatted.Substring(0, 3);
                    string major = phoneNumberUnformatted.Substring(3, 3);
                    string minor = phoneNumberUnformatted.Substring(6);

                    string phoneNumberFormatted = string.Format("{0}-{1}-{2}", areaCode, major, minor);

                    payment.ToMobileNumber = phoneNumberFormatted;

                    //Validate Payment

                    //Attempt to assign payment to Payee
                    var payee = _ctx.Users.FirstOrDefault(u => u.MobileNumber == payment.ToMobileNumber);

                    var transactionBatch = _ctx.TransactionBatches.FirstOrDefault(t => t.IsClosed == false);

                    if (transactionBatch == null)
                        transactionBatch = _ctx.TransactionBatches.Add(new TransactionBatch()
                        {
                            Id = Guid.NewGuid(),
                            CreateDate = System.DateTime.Now,
                            IsClosed = false

                        });

                    //Create withdraw transaction
                    payment.Transactions.Add(new Domain.Transaction()
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = payment.Id,
                        FromAccountId = payment.FromAccountId,
                        Amount = payment.PaymentAmount,
                        Status = TransactionStatus.Pending,
                        Category = TransactionCategory.Payment,
                        Type = TransactionType.Withdrawal,
                        TransactionBatchId = transactionBatch.Id,
                        CreateDate = System.DateTime.Now,
                        StandardEntryClass = payment.StandardEntryClass,
                        PaymentChannelType = payment.PaymentChannelType
                    });

                    transactionBatch.TotalNumberOfWithdrawals += 1;
                    transactionBatch.TotalWithdrawalAmount += payment.PaymentAmount;

                    if (payee != null  && payee.PaymentAccounts.Count > 0)
                    {
                        payment.ToAccountId = payee.PaymentAccounts[0].Id;

                        //Create deposit transaction
                        payment.Transactions.Add(new Domain.Transaction()
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.Id,
                            FromAccountId = payee.PaymentAccounts[0].Id,
                            Amount = payment.PaymentAmount,
                            Status = TransactionStatus.Pending,
                            Category = TransactionCategory.Payment,
                            Type = TransactionType.Deposit,
                            TransactionBatchId = transactionBatch.Id,
                            CreateDate = System.DateTime.Now,
                            StandardEntryClass = payment.StandardEntryClass,
                            PaymentChannelType = payment.PaymentChannelType
                        });

                        transactionBatch.TotalNumberOfDeposits += 1;
                        transactionBatch.TotalDepositAmount += payment.PaymentAmount;

                        logger.Log(LogLevel.Info, String.Format("Send SMS to Payee"));

                        //Send out SMS Message to payee
                        smsService.SendSMS(new SocialPayments.Services.DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("You received a payment for {0:C} from {1}.  The payment is complete. PdThx.me", payment.PaymentAmount, payment.FromMobileNumber),
                            MobileNumber = payment.ToMobileNumber
                        });
                        logger.Log(LogLevel.Info, String.Format("Send SMS to Payer"));

                        //Send out SMS Message to payer
                        smsService.SendSMS(new SocialPayments.Services.DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("Your payment for {0:C} to {1} is complete. PdThx.me", payment.PaymentAmount, payment.ToMobileNumber),
                            MobileNumber = payment.FromMobileNumber
                        });
                        //Send out confirmation email to payer
                        emailService.SendEmail(new SocialPayments.Services.DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "Confirmation of your payment to " + payment.ToMobileNumber + ".",
                            Body = String.Format("Your payment in the amount of {0:C} was delivered to {1}.", payment.PaymentAmount, payment.ToMobileNumber),
                            FromAddress = fromAddress,
                            ToAddress = payment.FromAccount.User.EmailAddress
                        });
                        emailService.SendEmail(new SocialPayments.Services.DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "You received a payment from " + payment.FromMobileNumber + ".",
                            Body = String.Format("Your received a payment of {0:C} from {1}. {2}", payment.PaymentAmount, payment.FromMobileNumber, payment.Comments),
                            FromAddress = fromAddress,
                            ToAddress = payee.EmailAddress
                        });

                        //Update Payment Status
                        payment.PaymentStatus = PaymentStatus.Pending;
                    }
                    else
                    {
                        logger.Log(LogLevel.Info, String.Format("Send SMS to Payee not found"));

                        var link = String.Format("http://beta.paidthx.me/mobile/{0}", payment.Id);
                        //Send out SMS Message to payee
                        smsService.SendSMS(new SocialPayments.Services.DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("{1} just sent you {0:C} using PaidThx.  Goto {2} to complete this transaction.", payment.PaymentAmount, payment.FromMobileNumber, link),
                            MobileNumber = payment.ToMobileNumber
                        });
                        logger.Log(LogLevel.Info, String.Format("Send SMS to Payer no payee"));

                        //Send out SMS Message to payer
                        smsService.SendSMS(new SocialPayments.Services.DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("Your payment request for {0:C} was submitted to an unregistered user at {1}. PdThx.me", payment.PaymentAmount, payment.ToMobileNumber),
                            MobileNumber = payment.FromMobileNumber
                        });
                        emailService.SendEmail(new SocialPayments.Services.DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "Confirmation of your payment to " + payment.ToMobileNumber + ".",
                            Body = String.Format("Your payment in the amount of {0:C} was delivered to {1}.", payment.PaymentAmount, payment.ToMobileNumber),
                            FromAddress = fromAddress,
                            ToAddress = payment.FromAccount.User.EmailAddress
                        });
                        emailService.SendEmail(new SocialPayments.Services.DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "Your recent payment to " + payment.ToMobileNumber + ".",
                            Body = String.Format("The recipient of your payment ({0}) does not have an account with PdThx.  We have sent their mobile number information about your payment and instructions to register.  Please help us ensure that your transaction is completed by reminding the recipient of your payment to register with us.  Thanks.", payment.ToMobileNumber),
                            FromAddress = fromAddress,
                            ToAddress = payment.FromAccount.User.EmailAddress
                        });
                    }
                    logger.Log(LogLevel.Info, String.Format("Updating Payment"));

                    payment.LastUpdatedDate = System.DateTime.Now;

                    _ctx.SaveChanges();

                    break;
            }
        }
    }
}
