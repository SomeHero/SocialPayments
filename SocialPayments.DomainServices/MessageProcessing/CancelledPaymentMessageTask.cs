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

                var message = ctx.Messages.FirstOrDefault(m => m.Id == messageId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Found", messageId));

                message.Status = PaystreamMessageStatus.CancelledPayment;
                message.LastUpdatedDate = System.DateTime.Now;

                if (message.Payment != null)
                {

                    foreach (var transaction in message.Payment.Transactions)
                    {
                        if (transaction.TransactionBatch != null)
                        {
                            _logger.Log(LogLevel.Info, String.Format("Removing Transaction {0} from Batch {1}", transaction.Id, transaction.TransactionBatchId));

                            transaction.TransactionBatch = null;
                        }
                        transaction.Status = TransactionStatus.Cancelled;
                        transaction.LastUpdatedDate = System.DateTime.Now;

                    }

                    message.Payment.PaymentStatus = PaymentStatus.Cancelled;
                }


                ctx.SaveChanges();

            }
        }
    }
}
