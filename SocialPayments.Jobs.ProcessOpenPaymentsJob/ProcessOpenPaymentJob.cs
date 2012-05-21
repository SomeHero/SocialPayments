using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;
using Quartz.Impl.Calendar;
using Quartz;
using Quartz.Impl;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;

namespace SocialPayments.Jobs.ProcessOpenPaymentsJob
{
    public class ProcessOpenPaymentJob: IJob
    {
        private readonly Context _ctx = new Context();
        private DomainServices.TransactionBatchService transactionBatchService;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public ProcessOpenPaymentJob()
        {
            transactionBatchService = new DomainServices.TransactionBatchService(_ctx, _logger);
        }
        
        public void Execute(JobExecutionContext context)
        {
            var payments = _ctx.Messages.Where(p => p.MessageStatus == Domain.MessageStatus.Pending);
            int numberOfDaysOpenThreshold = 10;
            foreach (var payment in payments)
            {
                var transactionBatch = transactionBatchService.GetOpenBatch();

                if (payment.CreateDate.AddDays(numberOfDaysOpenThreshold).Date > System.DateTime.Now.Date)
                {
                    //Create a transaction to deposit payment amount in payer's account
                    payment.Transactions.Add(new Domain.Transaction()
                    {
                        Amount = payment.Amount,
                        Category = Domain.TransactionCategory.Payment,
                        CreateDate = System.DateTime.Now,
                        FromAccountId = payment.SenderAccountId.Value,
                        PaymentChannelType = Domain.PaymentChannelType.Single,
                        //PaymentId = payment.Id,
                        StandardEntryClass = Domain.StandardEntryClass.Web,
                        Status = Domain.TransactionStatus.Pending,
                        TransactionBatchId = transactionBatch.Id,
                        Type = Domain.TransactionType.Deposit
                    });
                }
            }

        }
    }
}
