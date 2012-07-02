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
using System.Collections.ObjectModel;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class PaystreamMessagesController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/paystreammessage
        public HttpResponseMessage<MessageModels.MessageResponse> Get(string id)
        {
             _logger.Log(LogLevel.Info, String.Format("Getting Message {0} Started", id));

            using (var _ctx = new Context())
            {
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

                Domain.Message message = null;

                HttpResponseMessage<MessageModels.MessageResponse> responseMessage;

                try
                {
                    message = _messageServices.GetMessage(id);

                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Fatal, String.Format("Unhandled Exception Getting Message {0}. {1}", id, ex.Message));

                    responseMessage = new HttpResponseMessage<MessageModels.MessageResponse>(HttpStatusCode.InternalServerError);
                    responseMessage.ReasonPhrase = ex.Message;

                    return responseMessage;
                }

                responseMessage = new HttpResponseMessage<MessageModels.MessageResponse>(new MessageModels.MessageResponse()
                {
                    amount = message.Amount,
                    comments = message.Comments,
                    createDate = message.CreateDate.ToString("MM/dd/yyyy"),
                    direction = @"In",
                    Id = message.Id,
                    lastUpdatedDate = (message.LastUpdatedDate != null ? message.LastUpdatedDate.Value.ToString("MM/dd/yyyy") : ""),
                    latitude = message.Latitude,
                    longitutde = message.Longitude,
                    messageStatus = message.Status.ToString(),
                    messageType = message.MessageType.ToString(),
                    recipientName = (message.Recipient != null ? _userService.GetSenderName(message.Recipient) : ""),
                    recipientUri = message.RecipientUri,
                    senderName = (message.Sender != null ? _userService.GetSenderName(message.Sender) : ""),
                    senderUri = message.SenderUri,
                    transactionImageUri = message.senderImageUri
                },
                HttpStatusCode.OK);

                _logger.Log(LogLevel.Info, String.Format("Getting Message {0} Complete", id));

                return responseMessage;
            }
        }

        // GET /api/paystreammessage/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/messages
        public HttpResponseMessage<MessageModels.SubmitMessageResponse> Post(MessageModels.SubmitMessageRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("{0} - New Message Posted {1} {2} {3} {4}", request.apiKey, request.senderId, request.recipientUri, request.recipientFirstName, request.recipientLastName));

            using (var _ctx = new Context())
            {
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

                Domain.Message message = null;
                HttpResponseMessage<MessageModels.SubmitMessageResponse> responseMessage;

                var user = _userService.GetUserById(request.senderId);

                if (user == null)
                {
                    responseMessage = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(new MessageModels.SubmitMessageResponse()
                    {
                        isLockedOut = true,
                        numberOfPinCodeFailures = 0
                    }, HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = String.Format("Invalid Sender Id {0} specified in the request", request.senderId);

                    return responseMessage;
                }

                if (user.IsLockedOut)
                {
                    responseMessage = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(new MessageModels.SubmitMessageResponse()
                    {
                        isLockedOut = user.IsLockedOut,
                        numberOfPinCodeFailures = user.PinCodeFailuresSinceLastSuccess
                    }, HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = String.Format("Sender Account Locked");

                    return responseMessage;
                }
                if (!(_securityService.Encrypt(request.securityPin).Equals(user.SecurityPin)))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        responseMessage = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(new MessageModels.SubmitMessageResponse()
                        {
                            isLockedOut = user.IsLockedOut,
                            numberOfPinCodeFailures = user.PinCodeFailuresSinceLastSuccess
                        }, HttpStatusCode.BadRequest);

                        responseMessage.ReasonPhrase = String.Format("Security Pin Invalid.  Sender Account Locked.");
                    }
                    else
                    {
                        responseMessage = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(new MessageModels.SubmitMessageResponse()
                        {
                            isLockedOut = user.IsLockedOut,
                            numberOfPinCodeFailures = user.PinCodeFailuresSinceLastSuccess
                        }, HttpStatusCode.BadRequest);

                        responseMessage.ReasonPhrase = String.Format("Security Pin Invalid");
                    }
                    _userService.UpdateUser(user);

                    return responseMessage;
                }

                try
                {

                    message = _messageServices.AddMessage(request.apiKey, request.senderId, request.recipientUri, request.senderAccountId,
                        request.amount, request.comments, request.messageType, request.securityPin, request.latitude, request.longitude,
                       request.recipientFirstName, request.recipientLastName, request.recipientImageUri);

                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Fatal, String.Format("Exception Adding Message {0} {1} {2}. {3}", request.apiKey, request.senderId, request.recipientUri, ex.Message));

                    var innerException = ex.InnerException;

                    while (innerException != null)
                    {
                        _logger.Log(LogLevel.Fatal, String.Format("Inner Exception. {0}", ex.Message));

                        innerException = innerException.InnerException;
                    }
                    responseMessage = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.InternalServerError);
                    responseMessage.ReasonPhrase = ex.Message;

                    return responseMessage;
                }

                responseMessage = new HttpResponseMessage<MessageModels.SubmitMessageResponse>(HttpStatusCode.Created);
                //responseMessage.Headers.C

                return responseMessage;
            }
        }

        // POST /api/paystreammessages/{id}/cancel_payment
        [HttpPost]
        public HttpResponseMessage CancelPayment(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Cancel Payment Request Started {0}", id));

            using (var _ctx = new Context())
            {
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();


                HttpResponseMessage responseMessage;

                try
                {
                    _messageServices.CancelMessage(id);
                }
                catch (AccountLockedPinCodeFailures ex)
                {
                    responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = String.Format("Invalid Pincode.  Your account has been temporarily locked for {0} minutes", ex.LockOutInterval);

                    return responseMessage;
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Fatal, String.Format("Exception Adding Message {0}. {1}", id, ex.Message));

                    responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    responseMessage.ReasonPhrase = ex.Message;

                    return responseMessage;
                }

                _logger.Log(LogLevel.Debug, String.Format("Cancel Pending Payment Complete {0}", id));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
        // POST /api/paystreammessages/{id}/cancel_request
        [HttpPost]
        public HttpResponseMessage CancelRequest(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Cancel Request Started {0}", id));


            using (var _ctx = new Context())
            {
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

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

                message.Status = PaystreamMessageStatus.Cancelled;
                message.LastUpdatedDate = System.DateTime.Now;

                //Create Update Message 



                _ctx.SaveChanges();

                _logger.Log(LogLevel.Debug, String.Format("Cancel Request Complete {0} Ended", id));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
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
            _logger.Log(LogLevel.Info, String.Format("Accept Payment Request Started {0}", id));


            using (var _ctx = new Context())
            {
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

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

                if (message == null)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = "Invalid Message";

                    return responseMessage;
                }

                Domain.PaymentAccount paymentAccount = null;

                try
                {
                    paymentAccount = _paymentAccountServices.GetPaymentAccount(request.paymentAccountId);
                }
                catch (Exception ex)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = ex.Message;

                    return responseMessage;
                }

                if (paymentAccount == null)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = "Invalid Payment Account";

                    return responseMessage;
                }

                message.Status = PaystreamMessageStatus.Accepted;
                message.LastUpdatedDate = System.DateTime.Now;

                try
                {
                    _ctx.Messages.Add(new Message()
                    {
                        Amount = message.Amount,
                        ApiKey = message.ApiKey,
                        Comments = String.Format("Re: {0}", message.Comments),
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        Latitude = 0,
                        Longitude = 0,
                        Status = PaystreamMessageStatus.Processing,
                        MessageType = MessageType.Payment,
                        Recipient = message.Sender,
                        recipientFirstName = message.senderFirstName,
                        recipientLastName = message.senderLastName,
                        recipientImageUri = message.senderImageUri,
                        RecipientUri = message.SenderUri,
                        Sender = message.Recipient,
                        senderFirstName = message.recipientFirstName,
                        senderLastName = message.recipientLastName,
                        senderImageUri = message.recipientImageUri,
                        SenderAccount = paymentAccount,
                        SenderUri = message.RecipientUri,
                        WorkflowStatus = PaystreamMessageWorkflowStatus.Pending
                    });

                    _ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Processing Accept Payment Request {0}. {1}", id, ex.Message));

                    throw ex;
                }

                _logger.Log(LogLevel.Info, String.Format("Accept Payment Request Complete {0}", id));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
        // POST /api/paystreammessages/{id}/reject_request
        [HttpPost]
        public HttpResponseMessage RejectPaymentRequest(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Reject Payment Request Started {0}", id));


            using (var _ctx = new Context())
            {
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

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

                message.Status = PaystreamMessageStatus.Rejected;
                message.LastUpdatedDate = System.DateTime.Now;

                try
                {
                    //Create Update Message

                    _ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Processing Accept Payment Request {0}. {1}", id, ex.Message));

                    throw ex;
                }

                _logger.Log(LogLevel.Info, String.Format("Reject Payment Request Complete {0}", id));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
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
