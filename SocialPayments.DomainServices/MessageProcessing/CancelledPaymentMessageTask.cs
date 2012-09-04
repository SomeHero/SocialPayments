using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using NLog;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class CancelledPaymentMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Execute(Guid messageId)
        {
            using (Context ctx = new Context())
            {
                var messageService = new MessageServices();
                var transactionBatchService = new TransactionBatchService(ctx, _logger);

                var message = messageService.GetMessage(messageId);

                message.Status = PaystreamMessageStatus.CancelledPayment;

                if (message.Payment != null)
                {
                    //_logger.Log(LogLevel.Debug, String.Format("Removing {0} item(s) from batch", message.Payment.Transactions.Count));

                    //transactionBatchService.RemoveTransactionsFromBatch(message.Payment.Transactions);

                    //foreach (var transaction in message.Payment.Transactions)
                    //{
                    //    transaction.Status = TransactionStatus.Cancelled;
                    //    transaction.LastUpdatedDate = System.DateTime.Now;
                    //}

                    //message.Payment.PaymentStatus = PaymentStatus.Cancelled;
                    //message.LastUpdatedDate = System.DateTime.Now;

                    
                }

                ctx.SaveChanges();

            }
        }
    }
}
