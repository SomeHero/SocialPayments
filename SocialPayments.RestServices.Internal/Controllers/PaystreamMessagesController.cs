using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.RestServices.Internal.Models;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Configuration;
using NLog;
using System.Text.RegularExpressions;
using System.Data.Entity;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.DomainServices.CustomExceptions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class PaystreamMessagesController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"];

        // GET /api/paystreammessage
        public HttpResponseMessage<MessageModels.PagedResults> Get(int take, int skip, int page, int pageSize)
        {
            var messageServices = new DomainServices.MessageServices();
            List<Domain.Message> messages = null;
            int totalRecords = 0;
            HttpResponseMessage<MessageModels.PagedResults> response = null;

            try
            {
                messages = messageServices.GetPagedMessages(take, skip, page, pageSize, out totalRecords);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Paged Messages. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.PagedResults>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Paged Messages. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.PagedResults>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Paged Messages. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.PagedResults>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            return new HttpResponseMessage<MessageModels.PagedResults>(
                new MessageModels.PagedResults()
                {
                    TotalRecords = totalRecords,
                    Results = messages.Select(m => new MessageModels.MessageResponse()
                    {
                        amount = m.Amount,
                        comments = m.Comments,
                        createDate = m.CreateDate.ToString("MM/dd/yyyy"),
                        Id = m.Id,
                        lastUpdatedDate = (m.LastUpdatedDate != null ? m.LastUpdatedDate.Value.ToString("MM/dd/yyyy") : ""),
                        latitude = m.Latitude,
                        longitutde = m.Longitude,
                        messageStatus = m.Status.ToString(),
                        messageType = m.MessageType.ToString(),
                        recipientUri = m.RecipientUri,
                        senderUri = m.SenderUri
                    })
                }, HttpStatusCode.OK);
        }
        public HttpResponseMessage<MessageModels.MessageResponse> Get(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting Message {0} Started", id));

            var messageServices = new DomainServices.MessageServices();
            Domain.Message message = null;
            HttpResponseMessage<MessageModels.MessageResponse> response = null;

            try
            {
                message = messageServices.GetMessage(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Message {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.MessageResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Message {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.MessageResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Message {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.MessageResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<MessageModels.MessageResponse>(new MessageModels.MessageResponse()
            {
                amount = message.Amount,
                comments = message.Comments,
                createDate = message.CreateDate.ToString("MM/dd/yyyy"),
                Id = message.Id,
                lastUpdatedDate = (message.LastUpdatedDate != null ? message.LastUpdatedDate.Value.ToString("MM/dd/yyyy") : ""),
                latitude = message.Latitude,
                longitutde = message.Longitude,
                messageStatus = message.Status.ToString(),
                messageType = message.MessageType.ToString(),
                recipientUri = message.RecipientUri,
                senderUri = message.SenderUri,
                senderName = message.SenderName,
                transactionImageUri = message.TransactionImageUrl
            }, HttpStatusCode.OK);

            return response;
        }


        //POST /api/{userId}/PayStreamMessages/update_messages_seen
        public HttpResponseMessage UpdateMessagesSeen(MessageModels.MessageSeenUpdateRequest request)
        {
            var messagesServices = new DomainServices.MessageServices();
            HttpResponseMessage response = null;

            try
            {
                messagesServices.UpdateMessagesSeen(request.userId, request.messageIds);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Update Messages Seen. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Update Messages Seen. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Update Messages Seen. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        // POST /api/paystreammessages/donate
        public HttpResponseMessage<MessageModels.SubmitMessageResponse> Donate(MessageModels.SubmitDonateRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - Pledge Posted {1} {2} {3} {4}", request.apiKey, request.senderId, request.organizationId, request.recipientFirstName, request.recipientLastName));

            var messagesServices = new DomainServices.MessageServices();
            HttpResponseMessage<MessageModels.SubmitMessageResponse> response = null;
            Domain.Message message = null;
            try
            {
                message = messagesServices.Donate(request.apiKey, request.senderId, request.organizationId, request.organizationId, request.senderAccountId, request.amount, request.comments, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Donation. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Donation. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Donation. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(new MessageModels.SubmitMessageResponse()
            {

            }, HttpStatusCode.Created);

            return response;
        }
        // POST /api/message/accept_pledge
        public HttpResponseMessage<MessageModels.SubmitMessageResponse> AcceptPledge(MessageModels.SubmitPledgeRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - Pledge Posted {1} {2} {3} {4}", request.apiKey, request.onBehalfOfId, request.recipientUri, request.recipientFirstName, request.recipientLastName));

            DomainServices.MessageServices _messageServices = new DomainServices.MessageServices();
            Domain.Message message = null;
            HttpResponseMessage<MessageModels.SubmitMessageResponse> response = null;

            try
            {
                message = _messageServices.AcceptPledge(request.apiKey, request.senderId, request.onBehalfOfId, request.recipientUri, request.amount,
                    request.comments, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Pledge. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Pledge. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Pledge. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.Created);

            return response;
        }
        // api/message/route_message
        public HttpResponseMessage<List<MessageModels.MultipleURIResponse>> DetermineRecipient(MessageModels.MultipleURIRequest request)
        {
            var messageServices = new DomainServices.MessageServices();
            Dictionary<string, User> results = null;
            List<MessageModels.MultipleURIResponse> list = new List<MessageModels.MultipleURIResponse>();
            HttpResponseMessage<List<MessageModels.MultipleURIResponse>> response = null;

            try
            {
                results= messageServices.RouteMessage(request.recipientUris);

                foreach(var item in results)
                {
                    list.Add(new MessageModels.MultipleURIResponse() {
                        firstName = item.Value.FirstName,
                        lastName = item.Value.LastName,
                        userUri = item.Key
                    });
                }
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<MessageModels.MultipleURIResponse>>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<MessageModels.MultipleURIResponse>>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<MessageModels.MultipleURIResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            //TODO: fix this
            if (list.Count == 0)
            {
                //Ask user how they want to invite this user to PaidThx.
                return new HttpResponseMessage<List<MessageModels.MultipleURIResponse>>(HttpStatusCode.NoContent);
            }
            else
            {
                //Determine who to send it to.
                return new HttpResponseMessage<List<MessageModels.MultipleURIResponse>>(list, HttpStatusCode.OK);
            }
              
        }

        // POST /api/messages
        public HttpResponseMessage<MessageModels.SubmitMessageResponse> Post(MessageModels.SubmitMessageRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - New Message Posted {1} {2} {3} {4}", request.apiKey, request.senderId, request.recipientUri, request.recipientFirstName, request.recipientLastName));

            var messageServices = new DomainServices.MessageServices();
            HttpResponseMessage<MessageModels.SubmitMessageResponse> response = null;

            if (request.messageType.ToUpper() != @"PAYMENT" && request.messageType.ToUpper() != @"PAYMENTREQUEST")
            {
                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = String.Format("Message Type {0} Invalid. Message Type Must Be Payment or PaymentRequest", request.messageType);

                return response;
            }
           
            try
            {
                if(request.messageType.ToUpper() == @"PAYMENT")
                    messageServices.AddPayment(request.apiKey, request.senderId, request.recipientUri, request.senderAccountId, request.amount, request.comments,
                        request.latitude, request.longitude, request.recipientFirstName, request.recipientLastName, request.recipientImageUri, request.securityPin);
                if (request.messageType.ToUpper() == @"PAYMENTREQUEST")
                    messageServices.AddRequest(request.apiKey, request.senderId, request.recipientUri, request.senderAccountId, request.amount, request.comments,
                        request.latitude, request.longitude, request.recipientFirstName, request.recipientLastName, request.recipientImageUri, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.Created);
            return response;
        }

        // POST /api/paystreammessages/{id}/cancel_payment
        [HttpPost]
        public HttpResponseMessage CancelPayment(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Cancel Payment {0} Started", id));

            var messageServices = new DomainServices.MessageServices();
            HttpResponseMessage response = null;

            try {
                messageServices.CancelPayment(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Cancelling Payment {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Cancelling Payment {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Payment {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            _logger.Log(LogLevel.Info, String.Format("Cancel Payment {0} Complete", id));

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;


        }
        // POST /api/paystreammessages/{id}/cancel_request
        [HttpPost]
        public HttpResponseMessage CancelRequest(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Cancel Request {0} Started", id));

            var messageServices = new DomainServices.MessageServices();
            HttpResponseMessage response = null;

            try {
                messageServices.CancelPayment(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Request {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }


            _logger.Log(LogLevel.Info, String.Format("Cancel Request {0} Complete", id));

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;

        }
        // POST /api/paystreammessages/{id}/refund_payment
        [HttpPost]
        public HttpResponseMessage RefundPayment(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }
        // POST /api/paystreammessages/{id}/accept_request
        [HttpPost]
        public HttpResponseMessage AcceptPaymentRequest(string id, MessageModels.AcceptPaymentRequestModel request)
        {
            _logger.Log(LogLevel.Info, String.Format("Accept Payment Request Started {0}", id));

            var messageServices = new DomainServices.MessageServices();
            HttpResponseMessage response = null;

            try
            {
                messageServices.AcceptPaymentRequest(id, request.userId, request.paymentAccountId, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Request {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            _logger.Log(LogLevel.Info, String.Format("Accept Payment Request Complete {0}", id));

            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }
        // POST /api/paystreammessages/{id}/reject_request
        [HttpPost]
        public HttpResponseMessage RejectPaymentRequest(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Reject Payment Request Started {0}", id));

            var messageServices = new DomainServices.MessageServices();
            HttpResponseMessage response = null;

            try
            {
                messageServices.RejectPaymentRequest(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Rejecting Payment Request {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Rejecting Payment Request {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Rejecting Payment Request {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }


            _logger.Log(LogLevel.Info, String.Format("Reject Payment Request Complete {0}", id));

            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
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
