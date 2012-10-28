using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class TransactionServices
    {
        public Domain.Transaction AddTransaction(string transactionTypeName, double amount, string routingNumber, string accountNumber, string nameOnAccount,
            string accountTypeName, string individualIdentifier)
        {
            using (var ctx = new Context())
            {
                var transactionBatch = ctx.TransactionBatches
                    .OrderByDescending(b => b.CreateDate)
                    .FirstOrDefault(b => b.IsClosed == false);

                Domain.AccountType accountType = Domain.AccountType.Checking;

                if (accountTypeName == "Savings")
                    accountType = Domain.AccountType.Savings;

                Domain.TransactionType transactionType = Domain.TransactionType.Deposit;

                if (transactionTypeName == "Withdrawal")
                    transactionType = Domain.TransactionType.Withdrawal;

                var transaction = ctx.Transactions.Add(new Domain.Transaction()
                {
                    AccountNumber = accountNumber,
                    AccountType = accountType,
                    ACHTransactionId = "",
                    Amount = amount,
                    CreateDate = System.DateTime.Now,
                    NameOnAccount = nameOnAccount,
                    Id = Guid.NewGuid(),
                    PaymentChannelType = Domain.PaymentChannelType.Single,
                    RoutingNumber = routingNumber,
                    StandardEntryClass = Domain.StandardEntryClass.Web,
                    Status = Domain.TransactionStatus.Pending,
                    Type = transactionType,
                    IndividualIdentifier = individualIdentifier,
                    TransactionBatch = transactionBatch
                });

                switch (transactionType)
                {
                    case Domain.TransactionType.Deposit:
                        transactionBatch.TotalNumberOfDeposits += 1;
                        transactionBatch.TotalDepositAmount += transaction.Amount;

                        break;
                    case Domain.TransactionType.Withdrawal:
                        transactionBatch.TotalNumberOfWithdrawals += 1;
                        transactionBatch.TotalWithdrawalAmount += transaction.Amount;

                        break;
                }

                ctx.SaveChanges();

                return transaction;

            }
        }
        public List<Domain.Transaction> GetTransactions(string withStatus)
        {
            Domain.TransactionStatus statusCode = Domain.TransactionStatus.Pending;

            if (withStatus == "SentToBank")
                statusCode = Domain.TransactionStatus.Processed;
            if (withStatus == "Complete")
                statusCode = Domain.TransactionStatus.Complete;
            if (withStatus == "Pending")
                statusCode = Domain.TransactionStatus.Pending;
            if (withStatus == "Cancelled")
                statusCode = Domain.TransactionStatus.Cancelled;

            using (var ctx = new Context())
            {
                var transactions = ctx.Transactions
                    .Include("Payment")
                    .Where(t => t.TransactionStatusId.Equals((int)statusCode))
                    .ToList<Domain.Transaction>();

                return transactions;
            }
        }
        public void UpdateTransactionStatus(Guid id, string newStatus)
        {
            using (var ctx = new Context())
            {
                var transaction = ctx.Transactions
                    .FirstOrDefault(t => t.Id == id);

                transaction.Status = Domain.TransactionStatus.Complete;

                if (transaction.Type == Domain.TransactionType.Deposit)
                {
                    if (transaction.Payment != null)
                    {
                        transaction.Payment.PaymentStatus = Domain.PaymentStatus.Complete;

                        if (transaction.Payment.Message != null)
                            transaction.Payment.Message.Status = Domain.PaystreamMessageStatus.CompletePayment;
                    }
                }

                ctx.SaveChanges();
            }
        }
    }
}
