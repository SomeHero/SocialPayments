using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
using System.Net;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class TransactionsController : ApiController
    {
        // GET /api/transactions
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/transactions/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/transactions
        public HttpResponseMessage<Models.TransactionModels.SubmitTransactionResponse> Post(string apiKey, Models.TransactionModels.SubmitTransactionRequest request)
        {
            using (var ctx = new Context())
            {
                var transactionBatch = ctx.TransactionBatches
                    .OrderByDescending(b => b.CreateDate)
                    .FirstOrDefault(b => b.IsClosed == false);

                Domain.AccountType accountType = Domain.AccountType.Checking;

                if (request.AccountType == "Savings")
                    accountType = Domain.AccountType.Savings;

                Domain.TransactionType transactionType = Domain.TransactionType.Deposit;

                if (request.TransactionType == "Withdrawal")
                    transactionType = Domain.TransactionType.Withdrawal;

                var transaction = ctx.Transactions.Add(new Domain.Transaction()
                {
                    AccountNumber = request.AccountNumber,
                    AccountType = accountType,
                    ACHTransactionId = "",
                    Amount = request.Amount,
                    CreateDate = System.DateTime.Now,
                    NameOnAccount = request.NameOnAccount,
                    Id = Guid.NewGuid(),
                    PaymentChannelType = Domain.PaymentChannelType.Single,
                    RoutingNumber = request.RoutingNumber,
                    StandardEntryClass = Domain.StandardEntryClass.Web,
                    Status = Domain.TransactionStatus.Pending,
                    Type = transactionType,
                    IndividualIdentifier = request.IndividualIdentifier,
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

                return new HttpResponseMessage<Models.TransactionModels.SubmitTransactionResponse>(new Models.TransactionModels.SubmitTransactionResponse()
                {
                    TransactionId = transaction.Id
                }, HttpStatusCode.Created);
            }
        }

        // PUT /api/transactions/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/transactions/5
        public void Delete(int id)
        {
        }
    }
}
