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

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class PaystreamMessagesController : ApiController
    {
        private static IDbContext _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx);
        private static IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
        private DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
        
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
           _logger.Log(LogLevel.Info,String.Format("{0} - New Message Posted {1} {2}", request.apiKey, request.senderUri, request.recipientUri));

           Domain.Message message = null;
           HttpResponseMessage responseMessage;

           try {

               message = _messageServices.AddMessage(request.apiKey, request.senderUri, request.recipientUri, request.senderAccountId,
                   request.amount, request.comments, request.messageType, request.securityPin);
                
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Exception Adding Message {0} {1} {2}. {3}", request.apiKey, request.senderUri, request.recipientUri, ex.Message));

                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMessage.ReasonPhrase = ex.Message;

                return responseMessage;
            }
           if (message != null)
           {
               _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", message.Id.ToString());
           }

            responseMessage = new HttpResponseMessage(HttpStatusCode.Created);
            //responseMessage.Headers.C

            return responseMessage;
        }

        // POST /api/paystreammessages/{id}/cancel_payment
        public HttpResponseMessage CancelPayment(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/refund_payment
        public HttpResponseMessage RefundPayment(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/accept_request
        public HttpResponseMessage AcceptPaymentRequest(string id, MessageModels.AcceptPaymentRequestModel request)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/reject_request
        public HttpResponseMessage RejectPaymentReqeust(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/ignore_request
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
