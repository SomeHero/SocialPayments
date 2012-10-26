using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.IO;
using System.Configuration;
using NLog;

namespace SocialPayments.Jobs.ProcessSettlementProcessor
{
    public class SettlementProcessorJob: IJob
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SettlementProcessorJob()
        {}

        public void Execute(JobExecutionContext context)
        {
            DateTime jobStartTime = System.DateTime.Now;

            _logger.Log(LogLevel.Info, String.Format("Starting Settlement Job {0}", jobStartTime));

            try
            {
                var transactionService = new Services.TransactionServices();
                //grab all of the trasnactions that are in the SentToBank status
                //need service to grab all transactions that were SentToBank
                var transactions = transactionService.GetTransactionsWithStatusSentToBank();

                _logger.Log(LogLevel.Info, String.Format("Found {0} Transactions to Process", transactions.Count));

                //foreach payment
                foreach (var transaction in transactions)
                {
                    //determine via the bank whether payment should be settled
                    //for now use 1
                    _logger.Log(LogLevel.Info, String.Format("Updating Transacation {0} to Sent to Bank  ", transaction.Id));

                    transactionService.UpdateTransactionStatusToComplete(transaction.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception in Settlement Job Processor. Exception: {0}.  Stack Trace: {1}", ex.Message, ex.StackTrace));
                throw ex;
            }
            _logger.Log(LogLevel.Info, String.Format("Ending Settlement Job {0}", jobStartTime));
           
        }

        private void CloseOpenBatch()
        {

        }
    }
}
