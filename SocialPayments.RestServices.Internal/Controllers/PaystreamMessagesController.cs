using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using SocialPayments.RestServices.Internal.Models;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Configuration;
using NLog;
using System.Text.RegularExpressions;
using System.Data.Entity;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class PaystreamMessagesController : ApiController
    {
        private static IDbContext _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
        private static DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
        private static DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
        private static DomainServices.TransactionBatchService _transactionBatchService =
            new DomainServices.TransactionBatchService(_ctx, _logger);

        // GET /api/paystreammessage
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/paystreammessage/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/messages
        public HttpResponseMessage Post(MessageModels.SubmitMessageRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - New Message Posted {1} {2} {3} {4}", request.apiKey, request.senderUri, request.recipientUri, request.recipientFirstName, request.recipientLastName));

            Domain.Message message = null;
            HttpResponseMessage responseMessage;

            try
            {

                message = _messageServices.AddMessage(request.apiKey, request.senderUri, request.recipientUri, request.senderAccountId,
                    request.amount, request.comments, request.messageType, request.securityPin, request.latitude, request.longitude,
                   request.recipientFirstName, request.recipientLastName, request.recipientImageUri);

            }
            catch (AccountLockedPinCodeFailures ex)
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                responseMessage.ReasonPhrase = String.Format("Invalid Pincode.  Your account has been temporarily locked for {0} minutes", ex.LockOutInterval);

                return responseMessage;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Exception Adding Message {0} {1} {2}. {3}", request.apiKey, request.senderUri, request.recipientUri, ex.Message));

                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMessage.ReasonPhrase = ex.Message;

                return responseMessage;
            }

            responseMessage = new HttpResponseMessage(HttpStatusCode.Created);
            //responseMessage.Headers.C

            return responseMessage;
        }

        // POST /api/paystreammessages/{id}/cancel_payment
        [HttpPost]
        public HttpResponseMessage CancelPayment(string id)
        {
            _logger.Log(LogLevel.Debug, String.Format("Cancel Payment {0} Started", id));

            try
            {
                _messageServices.CancelMessage(id);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Payment {0}. {1}", id, ex.Message));

                var messageResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                messageResponse.ReasonPhrase = ex.Message;

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            _logger.Log(LogLevel.Debug, String.Format("Cancel Payment {0} Ended", id));

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/refund_payment
        [HttpPost]
        public HttpResponseMessage RefundPayment(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/accept_request
        [HttpPost]
        public HttpResponseMessage AcceptPaymentRequest(string id, MessageModels.AcceptPaymentRequestModel request)
        {
            Domain.Message message;

            try
            {
                message = _messageServices.GetMessage(id);
            }
            catch (Exception ex)
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                responseMessage.ReasonPhrase = ex.Message;

                return responseMessage;
            }

            _transactionBatchService.BatchTransactions(message);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/reject_request
        [HttpPost]
        public HttpResponseMessage RejectPaymentReqeust(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/ignore_request
        [HttpPost]
        public HttpResponseMessage IgnorePaymentRequest(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // PUT /api/paystreammessage/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/paystreammessage/5
        public void Delete(int id)
        {
        }

    }
}
