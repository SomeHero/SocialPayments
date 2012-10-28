using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.DomainServices.CustomExceptions;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class TransactionsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        //GET /api/transactions
        [HttpGet]
        public HttpResponseMessage Get(string withStatus)
        {
            var transactionService = new DomainServices.TransactionServices();
            List<Domain.Transaction> transactions = null;

            try
            {
                transactions = transactionService.GetTransactions(withStatus);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Batching Transaction.  Exception {0}.", ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Batching Transaction.  Exception {0}.", ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Batching Transaction.  Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<List<Models.TransactionModels.TransactionResponse>>(HttpStatusCode.OK, transactions.Select(t => new Models.TransactionModels.TransactionResponse()
            {
                Id = t.Id,
                AccountNumber = t.AccountNumber,
                AccountType = t.AccountType.ToString(),
                ACHTransactionId = t.ACHTransactionId,
                Amount = t.Amount,
                CreateDate = t.CreateDate.ToString(),
                IndividualIdentifier = t.IndividualIdentifier,
                LastUpdatedDate = t.LastUpdatedDate.ToString(),
                NameOnAccount = t.NameOnAccount,
                PaymentChannelType = t.PaymentChannelType.ToString(),
                RoutingNumber = t.RoutingNumber,
                StandardEntryClass = t.StandardEntryClass.ToString(),
                Status = t.Status.ToString(),
                Type = t.Type.ToString()
                
            }).ToList<Models.TransactionModels.TransactionResponse>());

        }
        // POST /api/transactions
        [HttpPost]
        public HttpResponseMessage Post(string apiKey, Models.TransactionModels.SubmitTransactionRequest request)
        {
            var transactionService = new DomainServices.TransactionServices();
            Domain.Transaction transaction = null;

            try
            {
                transaction = transactionService.AddTransaction(request.TransactionType, request.Amount, request.RoutingNumber, request.AccountNumber, request.NameOnAccount,
                    request.AccountType, request.IndividualIdentifier);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Batching Transaction.  Exception {0}.", ex.Message));
                
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Batching Transaction.  Exception {0}.",  ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Batching Transaction.  Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


            return Request.CreateResponse<Models.TransactionModels.SubmitTransactionResponse>(HttpStatusCode.OK, new Models.TransactionModels.SubmitTransactionResponse()
            {
                TransactionId = transaction.Id
            });

        }
         // POST /api/transaction/{id}
        [HttpPost]
        public HttpResponseMessage UpdateTransactionStatus(Guid id)
        {
            var transactionService = new DomainServices.TransactionServices();

            try
            {
                transactionService.UpdateTransactionStatus(id, "Complete");
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Updating Transaction Status.  Exception {0}.", ex.Message));
                
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Updating Transaction Status.  Exception {0}.",  ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Updating Transaction Status.  Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


            return Request.CreateResponse(HttpStatusCode.OK);
       

        }
    }
}
