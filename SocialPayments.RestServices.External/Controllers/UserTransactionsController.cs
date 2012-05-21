using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.External.Models;
using System.Net;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using System.Data.Entity;

namespace SocialPayments.RestServices.External.Controllers
{
    public class UserTransactionsController : ApiController
    {
        private Context _ctx = new Context();
        private SecurityService _securityService = new SecurityService();

        // GET /api/users/{id}/transactions
        public HttpResponseMessage<List<TransactionModels.TransactionResponse>> Get(string id)
        {
            var user = GetUser(id);

            if (user == null)
                return new HttpResponseMessage<List<TransactionModels.TransactionResponse>>(HttpStatusCode.NotFound);

            List<Transaction> transactions = _ctx.Transactions
                .Include("FromAccount")
                .Where(t => t.UserId.Equals(user.UserId))
                .OrderByDescending(t => t.CreateDate)
                .ToList<Transaction>(); ;

            var messages = transactions.Select(t => new TransactionModels.TransactionResponse()
            {
                achTransactionInformation = new TransactionModels.ACHTransactionInformation()
                {
                    paymentChannel= t.PaymentChannelType.ToString(),
                    standardEntryClass = t.StandardEntryClass.ToString(),
                    transactionType = t.Type.ToString(),
                    transactionDate = t.SentDate
                }, 
                amount = t.Amount,
                createDate = t.CreateDate,
                lastUpdatedDate = t.LastUpdatedDate,
                messageId = "",
                transactedAccount = new AccountModels.AccountResponse()
                {
                    AccountNumber = _securityService.Decrypt(t.FromAccount.AccountNumber),
                    RoutingNumber = _securityService.Decrypt(t.FromAccount.RoutingNumber),
                    NameOnAccount = _securityService.Decrypt(t.FromAccount.NameOnAccount),
                    AccountType = t.FromAccount.AccountType.ToString(),
                    Id = t.FromAccount.Id.ToString(),
                    UserId = t.FromAccount.UserId.ToString()
                },
                transactionCategory = t.Category.ToString(),
                transactionId = t.Id.ToString(),
                transactionStatus = t.Status.ToString()
            }).ToList();

            return new HttpResponseMessage<List<TransactionModels.TransactionResponse>>(messages, HttpStatusCode.OK);
        }
        private User GetUser(string id)
        {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            var user = _ctx.Users.FirstOrDefault(u => u.UserId.Equals(userId));

            if (user == null)
                return null;

            return user;
        }
    }
}
