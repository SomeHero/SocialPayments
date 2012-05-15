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

        public ProcessOpenPaymentJob()
        {
            transactionBatchService = new DomainServices.TransactionBatchService(_ctx);
        }
        
        public void Execute(JobExecutionContext context)
        {
            var payments = _ctx.Payments.Where(p => p.PaymentStatus == Domain.PaymentStatus.Pending);
            int numberOfDaysOpenThreshold = 10;
            foreach (var payment in payments)
            {
                var transactionBatch = transactionBatchService.GetOpenBatch();

                if (payment.PaymentDate.AddDays(numberOfDaysOpenThreshold).Date > System.DateTime.Now.Date)
                {
                    //Create a transaction to deposit payment amount in payer's account
                    payment.Transactions.Add(new Domain.Transaction()
                    {
                        Amount = payment.PaymentAmount,
                        Category = Domain.TransactionCategory.Payment,
                        CreateDate = System.DateTime.Now,
                        FromAccountId = payment.FromAccountId,
                        PaymentChannelType = payment.PaymentChannelType,
                        //PaymentId = payment.Id,
                        StandardEntryClass = payment.StandardEntryClass,
                        Status = Domain.TransactionStatus.Pending,
                        TransactionBatchId = transactionBatch.Id,
                        Type = Domain.TransactionType.Deposit
                    });
                }
            }

        }
    }
}
