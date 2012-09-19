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
                try
                {
                    var messageService = new MessageServices();
                    var transactionBatchService = new TransactionBatchService(ctx, _logger);

                    var message = ctx.Messages.FirstOrDefault(m => m.Id == messageId);

                    if (message == null)
                        throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Found", messageId));

                    ctx.Messages.Attach(message);

                    var transactionBatch = transactionBatchService.GetOpenBatch();
                        throw new CustomExceptions.BadRequestException("Unable to Get Open Batch {0}");
                    ctx.TransactionBatches.Attach(transactionBatch);

                    message.Status = PaystreamMessageStatus.CancelledPayment;
                    message.LastUpdatedDate = System.DateTime.Now;

                    if (message.Payment != null)
                    {

                        foreach (var transaction in message.Payment.Transactions)
                        {
                            var item = transactionBatch.Transactions.FirstOrDefault(t => t.Id == transaction.Id);

                            if (item != null)
                            {
                                _logger.Log(LogLevel.Info, String.Format("Removing Transaction {0} from Batch {1}", item.Id, transactionBatch.Id));

                                item.TransactionBatchId = null;

                                if (item.Type == TransactionType.Deposit)
                                {
                                    transactionBatch.TotalNumberOfDeposits -= 1;
                                    transactionBatch.TotalDepositAmount -= transaction.Amount;
                                }
                                if (item.Type == TransactionType.Withdrawal)
                                {
                                    transactionBatch.TotalNumberOfWithdrawals -= 1;
                                    transactionBatch.TotalWithdrawalAmount -= transaction.Amount;
                                }
                            }
                            transaction.Status = TransactionStatus.Cancelled;
                            transaction.LastUpdatedDate = System.DateTime.Now;

                        }

                        message.Payment.PaymentStatus = PaymentStatus.Cancelled;
                    }


                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Cancelled Payment Message Task. Exception: {0}. Stack Trace: {1}", ex.Message, ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Cancelled Payment Message Task. Inner Exception: {0}. Stack Trace: {1}", innerException.Message, innerException.StackTrace));
                        innerException = innerException.InnerException;
                    }
                }

            }
        }
    }
}
