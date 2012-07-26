using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Text.RegularExpressions;
using NLog;
using SocialPayments.DataLayer.Interfaces;
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class TransactionBatchService
    {
        private IDbContext _ctx;
        private Logger _logger;

        public TransactionBatchService(IDbContext context, Logger logger)
        {
            _ctx = context;
            _logger = logger;
        }
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
                                       CreateDate = System.DateTime.Now,
                                       Id = Guid.NewGuid(),
                                       IsClosed = false,
                                       TotalDepositAmount = 0,
                                       TotalNumberOfDeposits = 0,
                                       TotalWithdrawalAmount = 0,
                                       TotalNumberOfWithdrawals = 0,
                                       Transactions = new Collection<Transaction>()
                                   });
        }
        public TransactionBatch CloseOpenBatch()
        {
            var batch = _ctx.TransactionBatches.FirstOrDefault(t => t.IsClosed == false);

            batch.IsClosed = true;
            batch.LastDateUpdated = System.DateTime.Now;
            batch.ClosedDate = System.DateTime.Now;

            _ctx.TransactionBatches.Add(new TransactionBatch()
                {
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    IsClosed = false,
                    TotalDepositAmount = 0,
                    TotalNumberOfDeposits = 0,
                    TotalWithdrawalAmount = 0,
                    TotalNumberOfWithdrawals = 0,
                    Transactions = new Collection<Transaction>()
                });

            _ctx.SaveChanges();

            return batch;
        }
        public void AddTransactionsToBatch(Collection<Transaction> transactions)
        {
            var transactionBatch = GetOpenBatch();

            _logger.Log(LogLevel.Info, String.Format("Batch {0} Transactions in Batch {1}", transactions.Count, transactionBatch.Id));

            foreach (var transaction in transactions)
            {
                transactionBatch.Transactions.Add(transaction);

                if (transaction.Type == TransactionType.Deposit)
                {
                    transactionBatch.TotalNumberOfDeposits += 1;
                    transactionBatch.TotalDepositAmount += transaction.Amount;
                }
                if (transaction.Type == TransactionType.Withdrawal)
                {
                    transactionBatch.TotalNumberOfWithdrawals += 1;
                    transactionBatch.TotalWithdrawalAmount += transaction.Amount;
                }
            }

            _ctx.SaveChanges();
        }
        public TransactionBatch BatchTransactions()
        {
            var transactionBatch = GetOpenBatch();

            if (transactionBatch == null)
                throw new Exception("No batch found while batching transactions");

            transactionBatch.IsClosed = true;
            transactionBatch.ClosedDate = System.DateTime.Now;

            _logger.Log(LogLevel.Info, String.Format("Batching Transaction for transaction batch {0} with {1} transactions", transactionBatch.Id, transactionBatch.Transactions.Count()));

            foreach (var transaction in transactionBatch.Transactions)
            {
                try
                {
                    transaction.Payment.Message.Status = PaystreamMessageStatus.Complete;
                    transaction.Payment.PaymentStatus = PaymentStatus.Complete;
                    transaction.Status = TransactionStatus.Complete;
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Debug, String.Format("Warning Batching Transaction {0}. Exception: {1}", transaction.Id, ex.Message));
                }
            }
            
            _ctx.SaveChanges();

            return transactionBatch;
        }
        private User GetRecipient(string uri)
        {
            string phoneNumberUnformatted = Regex.Replace(uri, @"\D", string.Empty);

            if (phoneNumberUnformatted.Length != 10)
            {
                //logger.Log(LogLevel.Error, String.Format("To Mobile Number is not valid {0}", phoneNumberUnformatted));
                //throw new Exception(String.Format("To Mobile Number is not valid {0}", phoneNumberUnformatted));
            }

            string areaCode = phoneNumberUnformatted.Substring(0, 3);
            string major = phoneNumberUnformatted.Substring(3, 3);
            string minor = phoneNumberUnformatted.Substring(6);

            string phoneNumberFormatted = string.Format("{0}{1}{2}", areaCode, major, minor);

            var recipient = _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(u => u.MobileNumber.Equals(phoneNumberFormatted));

            return recipient;
        }


        public void RemoveTransactionsFromBatch(Collection<Transaction> transactions)
        {
            var transactionBatch = GetOpenBatch();

            foreach (var transaction in transactions)
            {
                var item = transactionBatch.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                  
                if(item != null)
                {
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
            }
        }
    }
}
