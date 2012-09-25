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

        //POST /api/batches/BatchServices/batch_transactions
        [HttpPost]
        public HttpResponseMessage BatchTransactions()
        {
            var transactionBatchServices = new DomainServices.TransactionBatchService();
            var formattingServices = new DomainServices.FormattingServices();
            Domain.TransactionBatch transactionBatch = null;
            HttpResponseMessage response = null;

            try
            {
                transactionBatch = transactionBatchServices.CloseOpenBatch();

            }
            catch (Exception ex)
            {
                var error = new HttpError(ex.Message);
                //error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            response = Request.CreateResponse<BatchModels.BatchResponse>(HttpStatusCode.OK, new BatchModels.BatchResponse()
            {
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
            });

            return response;
        }
    }
}
