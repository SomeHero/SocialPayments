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

        // GET /api/batches/{batchId}/transactions/GetPaged
        [HttpGet]
        public HttpResponseMessage GetPaged(string batchId, int take, int skip, int page, int pageSize)
        {
            var securityService = new DomainServices.SecurityService();
            var formattingService = new DomainServices.FormattingServices();
            var batchTransactionServices = new DomainServices.BatchTransactionsServices();
            
            Domain.TransactionBatch batch = null;
            HttpResponseMessage response = null;
            int totalRecords = 0;

            try
            {
                batch = batchTransactionServices.GetPagedTransactions(batchId, "", take, skip, page, pageSize, out totalRecords);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            response = Request.CreateResponse<TransactionModels.PagedResults>(HttpStatusCode.OK,
                new TransactionModels.PagedResults()
                {
                    TotalRecords = totalRecords,
                    Results = batch.Transactions.Select(t => new TransactionModels.TransactionResponse()
                    {
                        ACHTransactionId = t.ACHTransactionId,
                        Amount = t.Amount,
                        CreateDate = formattingService.FormatDateTimeForJSON(t.CreateDate),
                        Id = t.Id,
                        LastUpdatedDate = formattingService.FormatDateTimeForJSON(t.LastUpdatedDate),
                        AccountNumber = "****" + securityService.GetLastFour(securityService.Decrypt(t.AccountNumber)),
                        AccountType = t.AccountType.ToString(),
                        NameOnAccount = securityService.Decrypt(t.NameOnAccount),
                        RoutingNumber = securityService.Decrypt(t.RoutingNumber),
                        PaymentChannelType = t.PaymentChannelType.ToString(),
                        StandardEntryClass = t.StandardEntryClass.ToString(),
                        Status = t.Status.ToString(),
                        Type = t.Type.ToString()
                    })
                });

            return response;
        }

        // GET /api/batches/{batchId}/transactions
        [HttpGet]
        public HttpResponseMessage Get(string batchId)
        {
            var securityService = new DomainServices.SecurityService();
            var formattingService = new DomainServices.FormattingServices();
            var batchTransactionServices = new DomainServices.BatchTransactionsServices();
            Domain.TransactionBatch batch = null;
            HttpResponseMessage response = null;

            try
            {
                batch = batchTransactionServices.GetBatchTransactions(batchId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batch Transactions {0}. Exception {1}. Stack Trace {2}", batchId, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            response = Request.CreateResponse(HttpStatusCode.OK, batch.Transactions.Select(t => new TransactionModels.TransactionResponse()
            {
                ACHTransactionId = t.ACHTransactionId,
                Amount = t.Amount,
                CreateDate = formattingService.FormatDateTimeForJSON(t.CreateDate),
                Id = t.Id,
                LastUpdatedDate = formattingService.FormatDateTimeForJSON(t.LastUpdatedDate),
                AccountNumber =  "****" + securityService.GetLastFour(securityService.Decrypt(t.AccountNumber)),
                AccountType = t.AccountType.ToString(),
                NameOnAccount = securityService.Decrypt(t.NameOnAccount),
                RoutingNumber = securityService.Decrypt(t.RoutingNumber),
                PaymentChannelType = t.PaymentChannelType.ToString(),
                StandardEntryClass = t.StandardEntryClass.ToString(),
                Status = t.Status.ToString(),
                Type = t.Type.ToString()
            }).ToList());

            return response;
        }
    }
}
