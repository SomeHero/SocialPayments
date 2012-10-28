using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Data.Entity;

namespace SocialPayments.DomainServices.UserPayPointProcessing
{

    public class AddUserPayPointTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Excecute(Guid userPayPointId)
        {
            _logger.Log(LogLevel.Info, string.Format("Processing Add User Pay Point for {0}, Starting.", userPayPointId));

            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);
                var transactionBatchServices = new DomainServices.TransactionBatchService(ctx, _logger);

                var payPoint = ctx.UserPayPoints
                    .Include("User")
                    .Include("User.PreferredReceiveAccount")
                    .FirstOrDefault(u => u.Id == userPayPointId);

                var messages = ctx.Messages
                    .Where(m => m.RecipientUri == payPoint.URI)
                    .ToList<Domain.Message>();

                //get the users preferred receive account
                var paymentAccount = payPoint.User.PreferredReceiveAccount;

                var transactionBatch = transactionBatchServices.GetOpenBatch();
                ctx.TransactionBatches.Attach(transactionBatch);


                //foreach of those messages
                //mark status as Processing
                //create deposit record in transaction table
                foreach (var message in messages)
                {
                    if (message.Status == PaystreamMessageStatus.SubmittedPayment || message.Status == PaystreamMessageStatus.NotifiedPayment
                            || message.Status == PaystreamMessageStatus.PendingPayment)
                    {


                        if (paymentAccount != null)
                        {
                            _logger.Log(LogLevel.Info, String.Format("Batch Deposit Transaction for Message {0} into PaymentAccount {1}.", message.Id, paymentAccount.Id));

                            try
                            {
                                message.Status = PaystreamMessageStatus.ProcessingPayment;
                                message.Payment.RecipientAccount = paymentAccount;

                                //Add the deposit transaction
                                ctx.Transactions.Add(new Domain.Transaction()
                                {
                                    AccountNumber = paymentAccount.AccountNumber,
                                    Amount = message.Payment.Amount,
                                    AccountType = Domain.AccountType.Checking,
                                    ACHTransactionId = "",
                                    CreateDate = System.DateTime.Now,
                                    Id = Guid.NewGuid(),
                                    IndividualIdentifier = userService.GetSenderName(message.Sender),
                                    NameOnAccount = paymentAccount.NameOnAccount,
                                    PaymentChannelType = PaymentChannelType.Single,
                                    RoutingNumber = paymentAccount.RoutingNumber,
                                    StandardEntryClass = StandardEntryClass.Web,
                                    Status = TransactionStatus.Pending,
                                    Type = TransactionType.Deposit,
                                    TransactionBatch = transactionBatch,
                                    Payment = message.Payment
                                });

                                ctx.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                _logger.Log(LogLevel.Error, String.Format("Exception Batching Deposit Transaction for Message {0} into PaymentAccount {1}. Exception: {2}", message.Id, paymentAccount.Id, ex.Message));
                            }

                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Info, String.Format("Paypoint is null."));

                    }
                }

            }

        }
    }
}
