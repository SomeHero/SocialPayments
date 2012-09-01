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
            var transactionService = new DomainServices.TransactionServices();
            HttpResponseMessage<Models.TransactionModels.SubmitTransactionResponse> response = null;
            Domain.Transaction transaction = null;

            try
            {
                transaction = transactionService.AddTransaction(request.TransactionType, request.Amount, request.RoutingNumber, request.AccountNumber, request.NameOnAccount,
                    request.AccountType, request.IndividualIdentifier);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Batching Transaction.  Exception {0}.", ex.Message));

                response = new HttpResponseMessage<Models.TransactionModels.SubmitTransactionResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Batching Transaction.  Exception {0}.",  ex.Message));

                response = new HttpResponseMessage<Models.TransactionModels.SubmitTransactionResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Batching Transaction.  Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<Models.TransactionModels.SubmitTransactionResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
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
