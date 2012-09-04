using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

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
    }
}
