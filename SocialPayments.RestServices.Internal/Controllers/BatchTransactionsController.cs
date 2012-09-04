using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Data.Entity;
using System.Net;
using SocialPayments.DomainServices.CustomExceptions;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class BatchTransactionsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/batches/{batchId}/transactions
        public HttpResponseMessage<List<TransactionModels.TransactionResponse>> Get(string batchId)
        {
            var securityService = new DomainServices.SecurityService();
            var formattingService = new DomainServices.FormattingServices();
            var batchTransactionServices = new DomainServices.BatchTransactionsServices();
            Domain.TransactionBatch batch = null;
            HttpResponseMessage<List<TransactionModels.TransactionResponse>> response = null;

            try
            {
                batch = batchTransactionServices.GetBatchTransactions(batchId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<TransactionModels.TransactionResponse>>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<TransactionModels.TransactionResponse>>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<TransactionModels.TransactionResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
            }

            response = new HttpResponseMessage<List<TransactionModels.TransactionResponse>>(batch.Transactions.Select(t => new TransactionModels.TransactionResponse()
            {
                ACHTransactionId = t.ACHTransactionId,
                Amount = t.Amount,
                CreateDate = formattingService.FormatDateTimeForJSON(t.CreateDate),
                Id = t.Id,
                LastUpdatedDate = formattingService.FormatDateTimeForJSON(t.LastUpdatedDate),
                PaymentAccount = new AccountModels.AccountResponse()
                {
                    AccountNumber = securityService.Decrypt(t.AccountNumber),
                    AccountType = t.AccountType.ToString(),
                    NameOnAccount = securityService.Decrypt(t.NameOnAccount),
                    RoutingNumber = securityService.Decrypt(t.RoutingNumber)//,
                    // UserId = t.FromAccount.UserId.ToString()
                },
                PaymentChannelType = t.PaymentChannelType.ToString(),
                StandardEntryClass = t.StandardEntryClass.ToString(),
                Status = t.Status.ToString(),
                Type = t.Type.ToString()
            }).ToList(), HttpStatusCode.OK);

            return response;
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
