using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class BatchTransactionsController : ApiController
    {
        // GET /api/batches/{batchId}/transactions
        public IEnumerable<TransactionModels.TransactionResponse> Get(string batchId)
        {
            using (var ctx = new Context())
            {
                var securityService = new DomainServices.SecurityService();
                var formattingService = new DomainServices.FormattingServices();

                Guid id;

                Guid.TryParse(batchId, out id);

                var batch = ctx.TransactionBatches
                    .Include("Transactions")
                    .Include("Transactions.FromAccount")
                    .FirstOrDefault(b => b.Id.Equals(id));

                return batch.Transactions.Select(t => new TransactionModels.TransactionResponse()
                {
                    ACHTransactionId = t.ACHTransactionId,
                    Amount = t.Amount,
                    Category = t.Category.ToString(),
                    CreateDate = formattingService.FormatDateTimeForJSON(t.CreateDate),
                    Id = t.Id,
                    LastUpdatedDate = formattingService.FormatDateTimeForJSON(t.LastUpdatedDate),
                    PaymentAccount = new AccountModels.AccountResponse()
                    {
                        AccountNumber = securityService.Decrypt(t.FromAccount.AccountNumber),
                        AccountType = t.FromAccount.AccountType.ToString(),
                        Id = t.FromAccount.Id.ToString(),
                        NameOnAccount = securityService.Decrypt(t.FromAccount.NameOnAccount),
                        Nickname = t.FromAccount.Nickname,
                        RoutingNumber = securityService.Decrypt(t.FromAccount.RoutingNumber)//,
                        // UserId = t.FromAccount.UserId.ToString()
                    },
                    PaymentChannelType = t.PaymentChannelType.ToString(),
                    StandardEntryClass = t.StandardEntryClass.ToString(),
                    Status = t.Status.ToString(),
                    Type = t.Type.ToString()
                });
            }
        }

        // GET /api/transactions/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/transactions
        public void Post(string value)
        {
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
