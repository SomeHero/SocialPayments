using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Configuration;
using SocialPayments.DataLayer;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class SubmittedDonationMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();
        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSetURL"];
        private string _mobileWebSiteEngageURl = ConfigurationManager.AppSettings["MobileWebSetURL"];

        public void Execute(Guid messageId)
        {
            using(var ctx = new Context())
            {
                try
                {
                    var transactionBatchServices = new DomainServices.TransactionBatchService();
                    var messageService = new DomainServices.MessageServices();

                    var message = messageService.GetMessage(messageId);
                    ctx.Messages.Attach(message);

                    //Batch Transacations
                    //Calculate the # of hold days
                    var holdDays = 0;
                    var scheduledProcessingDate = System.DateTime.Now.Date;
                    var verificationLevel = PaymentVerificationLevel.Verified;

                    //var senderName = _userServices.GetSenderName(message.Sender);

                    var transactionBatch = transactionBatchServices.GetOpenBatch();
                    ctx.TransactionBatches.Attach(transactionBatch);

                    //Create Payment
                    message.Payment = new Payment()
                    {
                        Amount = message.Amount,
                        ApiKey = message.ApiKey,
                        Comments = message.Comments,
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        PaymentStatus = PaymentStatus.Pending,
                        SenderAccount = message.SenderAccount,
                        HoldDays = holdDays,
                        ScheduledProcessingDate = scheduledProcessingDate,
                        PaymentVerificationLevel = verificationLevel,
                        Transactions = new List<Transaction>()
                    };

                    //Add the withdrawal transaction
                    message.Payment.Transactions.Add(new Domain.Transaction()
                    {
                        AccountNumber = message.SenderAccount.AccountNumber,
                        Amount = message.Payment.Amount,
                        AccountType = Domain.AccountType.Checking,
                        ACHTransactionId = "",
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IndividualIdentifier = message.RecipientUri,
                        NameOnAccount = message.SenderAccount.NameOnAccount,
                        PaymentChannelType = PaymentChannelType.Single,
                        RoutingNumber = message.SenderAccount.RoutingNumber,
                        StandardEntryClass = StandardEntryClass.Web,
                        Status = TransactionStatus.Pending,
                        Type = TransactionType.Withdrawal,
                        TransactionBatch = transactionBatch,
                        Payment = message.Payment
                    });

                    transactionBatch.TotalNumberOfWithdrawals += 1;
                    transactionBatch.TotalWithdrawalAmount += message.Payment.Amount;

                    message.WorkflowStatus = PaystreamMessageWorkflowStatus.Complete;
                    message.LastUpdatedDate = System.DateTime.Now;

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Donation Message Task. Exception: {0}. Stack Trace: {1}", ex.Message, ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Donation Message Task. Inner Exception: {0}. Stack Trace: {1}", innerException.Message, innerException.StackTrace));
                        innerException = innerException.InnerException;
                    }
                }

            }
        }
    }
}
