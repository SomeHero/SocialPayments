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
        [HttpGet]
        public HttpResponseMessage GetPaged(int take, int skip, int page, int pageSize)
        {
            var messageServices = new DomainServices.MessageServices();
            List<Domain.Message> messages = null;
            int totalRecords = 0;

            try
            {
                messages = messageServices.GetPagedMessages("", "1", take, skip, page, pageSize, out totalRecords);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Paged Messages. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Paged Messages. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Paged Messages. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<MessageModels.PagedResults>(HttpStatusCode.OK,
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
                });
        }

        [HttpGet]
        public HttpResponseMessage Get(Guid id)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting Message {0} Started", id));

            var messageServices = new DomainServices.MessageServices();
            Domain.Message message = null;

            try
            {
                message = messageServices.GetMessage(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Message {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Message {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));
                 
                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Message {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<MessageModels.MessageResponse>(HttpStatusCode.OK, new MessageModels.MessageResponse()
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
                recipientUriType = messageServices.GetURIType(message.RecipientUri).ToString(),
                recipientName = message.RecipientName,
                senderUri = message.SenderUri,
                senderName = message.SenderName,
                transactionImageUri = message.TransactionImageUrl
            });
        }


        //POST /api/{userId}/PayStreamMessages/update_messages_seen
        [HttpPost]
        public HttpResponseMessage UpdateMessagesSeen(MessageModels.MessageSeenUpdateRequest request)
        {
            var messagesServices = new DomainServices.MessageServices();

            try
            {
                messagesServices.UpdateMessagesSeen(request.userId, request.messageIds);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Update Messages Seen. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Update Messages Seen. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
           
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Update Messages Seen. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST /api/paystreammessages/donate
        [HttpPost]
        public HttpResponseMessage Donate(MessageModels.SubmitDonateRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - Pledge Posted {1} {2} {3} {4}", request.apiKey, request.senderId, request.organizationId, request.recipientFirstName, request.recipientLastName));

            var messagesServices = new DomainServices.MessageServices();
            Domain.Message message = null;

            try
            {
                message = messagesServices.Donate(request.apiKey, request.senderId, request.organizationId, request.organizationId, request.senderAccountId, request.amount, request.comments, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Donation. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Donation. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Donation. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }
        [HttpPost]
        public HttpResponseMessage ExpressPayment(string userId, string id, MessageModels.ExpressPaymentRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Express Payment Started Sender: {0} Message: {1} Sender Account: {2}", userId, id, request.sendAccountId));

            var messagesServices = new DomainServices.MessageServices();

            try
            {
                messagesServices.ExpressPayment(userId, id, request.sendAccountId, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Expressing Payment. Exception {0}.", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Expressing Payment. Exception {0}.", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Expressing Payment. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);

        }
        // POST /api/message/accept_pledge
        [HttpPost]
        public HttpResponseMessage AcceptPledge(MessageModels.SubmitPledgeRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - Pledge Posted {1} {2} {3} {4}", request.apiKey, request.onBehalfOfId, request.recipientUri, request.recipientFirstName, request.recipientLastName));

            DomainServices.MessageServices _messageServices = new DomainServices.MessageServices();
            Domain.Message message = null;

            try
            {
                message = _messageServices.AcceptPledge(request.apiKey, request.senderId, request.onBehalfOfId, request.recipientUri, request.amount,
                    request.comments, request.latitude, request.longitude, request.recipientFirstName, request.recipientLastName, request.recipientLastName,
                    request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Pledge. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Pledge. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Pledge. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }
        // api/message/route_message
        [HttpPost]
        public HttpResponseMessage DetermineRecipient(MessageModels.MultipleURIRequest request)
        {
            var messageServices = new DomainServices.MessageServices();
            Dictionary<string, User> results = null;
            List<MessageModels.MultipleURIResponse> list = new List<MessageModels.MultipleURIResponse>();

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
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Determining Recipient. Exception {0}.", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Determining Recipient. Exception {0}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            //TODO: fix this
            if (list.Count == 0)
            {
                //Ask user how they want to invite this user to PaidThx.
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            else
            {
                //Determine who to send it to.
                return Request.CreateResponse<List<MessageModels.MultipleURIResponse>>(HttpStatusCode.OK, list);
            }
              
        }

        // POST /api/messages
        [HttpPost]
        public HttpResponseMessage Post(MessageModels.SubmitMessageRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - New Message Posted {1} {2} {3} {4}", request.apiKey, request.senderId, request.recipientUri, request.recipientFirstName, request.recipientLastName));

            var messageServices = new DomainServices.MessageServices();

            if (request.messageType.ToUpper() != @"PAYMENT" && request.messageType.ToUpper() != @"PAYMENTREQUEST")
                throw new SocialPayments.DomainServices.CustomExceptions.BadRequestException(String.Format("Message Type {0} Invalid. Message Type Must Be Payment or PaymentRequest", request.messageType));
           
            try
            {
                if(request.messageType.ToUpper() == @"PAYMENT")
                    messageServices.AddPayment(request.apiKey, request.senderId, request.recipientUri, request.senderAccountId, request.amount, request.comments,
                        request.latitude, request.longitude, request.recipientFirstName, request.recipientLastName, request.recipientImageUri, request.securityPin, request.deliveryMethod);
                if (request.messageType.ToUpper() == @"PAYMENTREQUEST")
                    messageServices.AddRequest(request.apiKey, request.senderId, request.recipientUri, request.senderAccountId, request.amount, request.comments,
                        request.latitude, request.longitude, request.recipientFirstName, request.recipientLastName, request.recipientImageUri, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
 
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Determining Recipient. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        // POST /api/paystreammessages/{id}/cancel_payment
        [HttpPost]
        public HttpResponseMessage CancelPayment(string id, MessageModels.CancelPaymentRequestModel request)
        {
            _logger.Log(LogLevel.Info, String.Format("Cancel Payment {0} Started", id));

            var messageServices = new DomainServices.MessageServices();

            try {
                messageServices.CancelPayment(id, request.userId, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Cancelling Payment {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Cancelling Payment {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Payment {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            _logger.Log(LogLevel.Info, String.Format("Cancel Payment {0} Complete", id));

            return Request.CreateResponse(HttpStatusCode.OK);


        }
        // POST /api/paystreammessages/{id}/cancel_request
        [HttpPost]
        public HttpResponseMessage CancelRequest(string id, MessageModels.CancelPaymentRequestRequestModel request)
        {
            _logger.Log(LogLevel.Info, String.Format("Cancel Request {0} Started", id));

            var messageServices = new DomainServices.MessageServices();

            try {
                messageServices.CancelRequest(id, request.userId, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Request {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            _logger.Log(LogLevel.Info, String.Format("Cancel Request {0} Complete", id));

            return Request.CreateResponse(HttpStatusCode.OK);

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
 
            try
            {
                messageServices.AcceptPaymentRequest(id, request.userId, request.paymentAccountId, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Cancelling Request {0}. Exception {1}", id, ex.Message));


                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Cancelling Request {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Cancelling Request {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            _logger.Log(LogLevel.Info, String.Format("Accept Payment Request Complete {0}", id));

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/reject_request
        [HttpPost]
        public HttpResponseMessage RejectPaymentRequest(string id, MessageModels.RejectPaymentRequestModel request)
        {
            _logger.Log(LogLevel.Info, String.Format("Reject Payment Request Started {0}", id));

            var messageServices = new DomainServices.MessageServices();

            try
            {
                messageServices.RejectPaymentRequest(id, request.userId, request.securityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Rejecting Payment Request {0}. Exception {1}", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Rejecting Payment Request {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Rejecting Payment Request {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            _logger.Log(LogLevel.Info, String.Format("Reject Payment Request Complete {0}", id));

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        // POST /api/paystreammessages/{id}/ignore_request
        [HttpPost]
        public HttpResponseMessage IgnorePaymentRequest(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}
