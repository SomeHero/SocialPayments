using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
using System.Net;
using SocialPayments.RestServices.External.Models;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.External.Controllers
{
    public class TransactionsController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/transactions
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // GET /api/transactions/5
        public HttpResponseMessage<TransactionModels.TransactionResponse> Get(string id)
        {
            var transaction = GetTransaction(id);

            if (transaction == null)
                return new HttpResponseMessage<TransactionModels.TransactionResponse>(HttpStatusCode.NotFound);

            var transactionResponse = new TransactionModels.TransactionResponse()
            {
                achTransactionInformation = new TransactionModels.ACHTransactionInformation() {
                    paymentChannel = transaction.PaymentChannelType.ToString(),
                    standardEntryClass = transaction.StandardEntryClass.ToString(),
                    transactionBatchId = transaction.TransactionBatchId.ToString(),
                    transactionDate = transaction.SentDate,
                    transactionId = transaction.ACHTransactionId,
                    transactionType = transaction.Type.ToString()
                },
                amount = transaction.Amount,
                createDate =transaction.CreateDate,
                lastUpdatedDate = transaction.LastUpdatedDate,
                messageId = "",
                transactedAccount = new AccountModels.AccountResponse() {
                    
                },
                transactionCategory = transaction.Category.ToString(),
                transactionId = transaction.Id.ToString(),
                transactionStatus = transaction.Status.ToString()
            };

            return new HttpResponseMessage<TransactionModels.TransactionResponse>(transactionResponse, HttpStatusCode.OK);
        }

        private Transaction GetTransaction(string id)
        {
            Guid transactionId;

            Guid.TryParse(id, out transactionId);

            if (transactionId == null)
                return null;

            var transaction = _ctx.Transactions.FirstOrDefault(t => t.Id.Equals(transactionId));

            if (transaction == null)
                return null;

            return transaction;
        }

        // POST /api/transactions
        public HttpResponseMessage Post(string value)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // PUT /api/transactions/5
        public HttpResponseMessage Put(string id, string value)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // DELETE /api/transactions/5
        public HttpResponseMessage Delete(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }
    }
}
