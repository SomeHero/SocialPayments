using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.RestServices.Internal.Models;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class BatchController : ApiController
    {
        // GET /api/transactionbatchservices
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/transactionbatchservices/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/transactionbatchservices
        public void Post(string value)
        {
        }
        //POST /api/batches/BatchServices/batch_transactions
        public HttpResponseMessage<BatchModels.BatchResponse> BatchTransactions()
        {
            var transactionBatchServices = new DomainServices.TransactionBatchService();
            var formattingServices = new DomainServices.FormattingServices();
            Domain.TransactionBatch transactionBatch = null;
            HttpResponseMessage<BatchModels.BatchResponse> response = null;

            try
            {
                transactionBatch = transactionBatchServices.CloseOpenBatch();

            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<BatchModels.BatchResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<BatchModels.BatchResponse>(new BatchModels.BatchResponse() {
                ClosedDate = formattingServices.FormatDateTimeForJSON(transactionBatch.ClosedDate),
                CreateDate = formattingServices.FormatDateTimeForJSON(transactionBatch.CreateDate),
                Id = transactionBatch.Id,
                IsClosed = transactionBatch.IsClosed,
                LastDateUpdated = formattingServices.FormatDateTimeForJSON(transactionBatch.LastDateUpdated),
                TotalDepositAmount = transactionBatch.TotalDepositAmount,
                TotalWithdrawalAmount = transactionBatch.TotalWithdrawalAmount,
                TotalNumberOfDeposits = transactionBatch.TotalNumberOfDeposits,
                TotalNumberOfWithdrawals = transactionBatch.TotalNumberOfWithdrawals,
                Transactions = transactionBatch.Transactions.Select(t => new TransactionModels.TransactionResponse() {
                    AccountNumber = t.AccountNumber,
                    AccountType = t.AccountType.ToString(),
                    ACHTransactionId = t.ACHTransactionId,
                    Amount = t.Amount,
                    CreateDate = formattingServices.FormatDateTimeForJSON(t.CreateDate),
                    Id = t.Id,
                    LastUpdatedDate = formattingServices.FormatDateTimeForJSON(t.LastUpdatedDate),
                    NameOnAccount = t.NameOnAccount,
                    IndividualIdentifier = t.IndividualIdentifier,
                    PaymentChannelType = t.PaymentChannelType.ToString(),
                    RoutingNumber = t.RoutingNumber,
                    StandardEntryClass = t.StandardEntryClass.ToString(),
                    Status = t.Status.ToString(),
                    Type = t.Type.ToString(),
                }).ToList()
            }, HttpStatusCode.OK);

            return response;
        }

        // PUT /api/transactionbatchservices/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/transactionbatchservices/5
        public void Delete(int id)
        {
        }
    }
}
