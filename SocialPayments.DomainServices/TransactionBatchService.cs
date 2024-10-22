﻿using System;
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

        public TransactionBatchService()
        { }

        public TransactionBatchService(IDbContext context, Logger logger)
        {
            _ctx = context;
            _logger = logger;
        }
        private TransactionBatch AddTransactionBatch(TransactionBatch transactionBatch)
        {
            using (var ctx = new Context())
            {
                transactionBatch = _ctx.TransactionBatches.Add(transactionBatch);

                _ctx.SaveChanges();

                return transactionBatch;
            }
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
            using (var ctx = new Context())
            {
                return ctx.TransactionBatches.FirstOrDefault(t => t.IsClosed == false) ??
                                       AddTransactionBatch(new TransactionBatch()
                                       {
                                           CreateDate = System.DateTime.Now,
                                           Id = Guid.NewGuid(),
                                           IsClosed = false,
                                           TotalDepositAmount = 0,
                                           TotalNumberOfDeposits = 0,
                                           TotalWithdrawalAmount = 0,
                                           TotalNumberOfWithdrawals = 0,
                                           Transactions = new List<Transaction>()
                                       });
            }
        }
        public TransactionBatch CloseOpenBatch()
        {
            using (var ctx = new Context())
            {
                var batch = ctx.TransactionBatches
                    .Include("Transactions")
                    .FirstOrDefault(t => t.IsClosed == false);

                batch.IsClosed = true;
                batch.LastDateUpdated = System.DateTime.Now;
                batch.ClosedDate = System.DateTime.Now;

                ctx.TransactionBatches.Add(new TransactionBatch()
                    {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsClosed = false,
                        TotalDepositAmount = 0,
                        TotalNumberOfDeposits = 0,
                        TotalWithdrawalAmount = 0,
                        TotalNumberOfWithdrawals = 0,
                        Transactions = new List<Transaction>()
                    });

                ctx.SaveChanges();

                return batch;
            }
        }
        public void AddTransactionsToBatch(Collection<Transaction> transactions)
        {
            var transactionBatch = GetOpenBatch();

            _logger.Log(LogLevel.Info, String.Format("Adding {0} Transactions in Batch {1}", transactions.Count, transactionBatch.Id));

            foreach (var transaction in transactions)
            {
                transactionBatch.Transactions.Add(new Transaction()
                {
                    ACHTransactionId = "",
                    Amount = transaction.Amount,
                    RoutingNumber = transaction.RoutingNumber,
                    AccountNumber = transaction.AccountNumber,
                    NameOnAccount = transaction.NameOnAccount,
                    AccountType = Domain.AccountType.Checking,
                    Id = Guid.NewGuid(),
                    PaymentChannelType = PaymentChannelType.Single,
                    StandardEntryClass = StandardEntryClass.Web,
                    Status = TransactionStatus.Pending
                });

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
                    //transaction.Payment.Message.Status = PaystreamMessageStatus.Complete;
                    //transaction.Payment.PaymentStatus = PaymentStatus.Complete;
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


        public void RemoveTransactionsFromBatch(List<Transaction> transactions)
        {
           
            using(var ctx = new Context())
            {
                var transactionBatch = ctx.TransactionBatches.FirstOrDefault(t => t.IsClosed == false);

                _logger.Log(LogLevel.Info, String.Format("Removing {0} Transactions from Batch {1}", transactions.Count, transactionBatch.Id));

                foreach (var transaction in transactions)
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
                }

                ctx.SaveChanges();
            }
        }
        public void UpdateBatchTransactionsToSentToBank(Guid batchId)
        {
            using (var ctx = new Context())
            {
                var transactionBatch = ctx.TransactionBatches.FirstOrDefault(t => t.Id == batchId);

                foreach (var transaction in transactionBatch.Transactions)
                {
                    transaction.Status = TransactionStatus.Processed;
                    transaction.SentDate = System.DateTime.Now;

                    if (transaction.Payment != null)
                    {
                        if (transaction.Payment.RecipientAccount != null)
                        {
                            transaction.Payment.PaymentStatus = PaymentStatus.Processed;
                            if (transaction.Payment.Message != null)
                                transaction.Payment.Message.Status = PaystreamMessageStatus.ProcessedPayment;
                        }
                        else
                            transaction.Payment.PaymentStatus = PaymentStatus.Open;
                    }
                }

                ctx.SaveChanges();
            }

        }
    }
}
