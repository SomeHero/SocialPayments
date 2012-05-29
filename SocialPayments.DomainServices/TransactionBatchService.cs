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
        public void BatchTransactions(Message message)
        {
            var transactionBatch = GetOpenBatch();

            var withDrawalTransaction =
                new Domain.Transaction()
                {
                    Amount = message.Amount,
                    Category = Domain.TransactionCategory.Payment,
                    CreateDate = System.DateTime.Now,
                    FromAccountId = message.SenderAccountId.Value,
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    PaymentChannelType = Domain.PaymentChannelType.Single,
                    StandardEntryClass = Domain.StandardEntryClass.Web,
                    Status = Domain.TransactionStatus.Pending,
                    TransactionBatchId = transactionBatch.Id,
                    Type = Domain.TransactionType.Withdrawal,
                    UserId = message.SenderId,
                    Message = message,
                };

            transactionBatch.Transactions.Add(withDrawalTransaction);
            transactionBatch.TotalNumberOfWithdrawals += 1;
            transactionBatch.TotalWithdrawalAmount += withDrawalTransaction.Amount;

            Transaction deposit;

            if (message.Recipient != null && message.Recipient.PaymentAccounts.Count > 0)
            {
                _logger.Log(LogLevel.Info, String.Format("Found Recipient {0} for Message {1}", message.Recipient.UserId, message.Id));
                
                var depositTransaction = 
                    new Domain.Transaction()
                    {
                        Amount = message.Amount,
                        Category = Domain.TransactionCategory.Payment,
                        CreateDate = System.DateTime.Now,
                        FromAccountId = message.Recipient.PaymentAccounts[0].Id,
                        Id = Guid.NewGuid(),
                        MessageId = message.Id,
                        PaymentChannelType = Domain.PaymentChannelType.Single,
                        StandardEntryClass = Domain.StandardEntryClass.Web,
                        Status = Domain.TransactionStatus.Pending,
                        TransactionBatchId = transactionBatch.Id,
                        Type = Domain.TransactionType.Deposit,
                        UserId = message.Recipient.UserId,
                    };

                transactionBatch.Transactions.Add(depositTransaction);
                transactionBatch.TotalNumberOfDeposits += 1;
                transactionBatch.TotalDepositAmount += depositTransaction.Amount;

                message.Recipient = message.Recipient;
            }

            message.MessageStatus = Domain.MessageStatus.Pending;
            message.LastUpdatedDate = System.DateTime.Now;

            _ctx.SaveChanges();
                       
        }
        public void BatchTransactions(List<Transaction> transactions)
        {
            var transactionBatch = GetOpenBatch();

            foreach (var transaction in transactions)
            {
                transactionBatch.Transactions.Add(transaction);

                if (transaction.Type == TransactionType.Deposit)
                {
                    transactionBatch.TotalNumberOfDeposits += 1;
                    transactionBatch.TotalDepositAmount += transaction.Amount;
                }
                else
                {
                    transactionBatch.TotalNumberOfWithdrawals += 1;
                    transactionBatch.TotalWithdrawalAmount += transaction.Amount;
                }
            }

            _ctx.SaveChanges();
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
                    Transactions = new List<Transaction>()
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
            transactionBatch.Transactions.ForEach(t => t.Message.MessageStatus = MessageStatus.Completed);
            _ctx.SaveChanges();

            AddTransactionBatch(new TransactionBatch()
                                    {
                                        Id = Guid.NewGuid(),
                                        CreateDate = System.DateTime.Now,
                                        IsClosed = false
                                                            
                                    });
            return transactionBatch.Transactions;
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


        public void RemoveFromBatch(Message message)
        {
            foreach (var temp in message.Transactions)
            {
                var transactionBatch = GetOpenBatch();
                
                if (temp.Status == TransactionStatus.Submitted || temp.Status == TransactionStatus.Pending)
                {
                    var transaction = transactionBatch.Transactions
                        .FirstOrDefault(t => t.Id.Equals(temp.Id));

                    transaction.LastUpdatedDate = System.DateTime.Now;
                    transaction.Status = TransactionStatus.Cancelled;

                    transactionBatch.Transactions.Remove(temp);
                }
            }
        }
    }
}
