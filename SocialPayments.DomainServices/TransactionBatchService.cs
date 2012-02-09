using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices
{
    public class TransactionBatchService
    {
        private readonly Context _ctx = new Context();

        private TransactionBatch AddTransactionBatch(TransactionBatch transactionBatch)
        {
            transactionBatch = _ctx.TransactionBatches.Add(transactionBatch);

            _ctx.SaveChanges();

            return transactionBatch;
        }
        public List<TransactionBatch> GetBatches(Expression<Func<TransactionBatch, bool>> expression)
        {
            return GetBatches(expression, 0, 1);
        }
        public List<TransactionBatch> GetBatches(Expression<Func<TransactionBatch, bool>> expression, int pageIndex, int pageSize)
        {
            var model = _ctx.TransactionBatches
                .Where(expression);

            model = model.OrderByDescending(t => t.CreateDate);
                
                return model.ToList<TransactionBatch>();
        }
        public TransactionBatch GetOpenBatch()
        {
            return _ctx.TransactionBatches.FirstOrDefault(t => t.IsClosed == false) ??
                                   AddTransactionBatch(new TransactionBatch()
                                                                                                                               {
                                                                                                                                   Id = Guid.NewGuid(),
                                                                                                                                   CreateDate = System.DateTime.Now,
                                                                                                                                   IsClosed = false

                                                                                                                               });

        }

        public List<Transaction> BatchTransactions()
        {

            var transactionBatch = _ctx.TransactionBatches
                .Include("Transactions")
                .FirstOrDefault(t => t.IsClosed == false);

            if(transactionBatch == null)
                throw new Exception("No batch found while batching transactions");

            transactionBatch.TotalNumberOfDeposits = transactionBatch.Transactions.Where(t => t.Type == TransactionType.Deposit).Count();
            transactionBatch.TotalDepositAmount = transactionBatch.Transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);
            transactionBatch.TotalNumberOfWithdrawals = transactionBatch.Transactions.Where(t => t.Type == TransactionType.Withdrawal).Count();
            transactionBatch.TotalWithdrawalAmount = transactionBatch.Transactions.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount);
            transactionBatch.IsClosed = true;
            transactionBatch.ClosedDate = System.DateTime.Now;
            transactionBatch.Transactions.ForEach(t => t.Status = TransactionStatus.Submitted);
            //transactionBatch.Transactions.ForEach(t => t.Payment.Status = TransactionStatus.Complete);
            _ctx.SaveChanges();

            AddTransactionBatch(new TransactionBatch()
                                    {
                                        Id = Guid.NewGuid(),
                                        CreateDate = System.DateTime.Now,
                                        IsClosed = false
                                                            
                                    });
            return transactionBatch.Transactions;
        }
    }
}
