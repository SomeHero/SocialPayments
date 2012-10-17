using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using NLog;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.Domain.ExtensionMethods;
using SocialPayments.DomainServices.CustomExceptions;
using System.Configuration;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPayStreamMessagesController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static DomainServices.FormattingServices _formattingServices = new DomainServices.FormattingServices();

        // GET /api/Users/{userId}/PayStreamMessages
        [HttpGet]
        public HttpResponseMessage GetPaged(string userId, string type, int take, int skip, int page, int pageSize)
        {
            var messageServices = new DomainServices.MessageServices();
            var formattingServices = new DomainServices.FormattingServices();

            List<Domain.Message> messages = null;
            int totalRecords = 0;

            try
            {
                messages = messageServices.GetPagedMessages(userId, type, take, skip, page, pageSize, out totalRecords);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting User Paystream Messages Paged for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting User Paystream Messages Paged for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting User Paystream Messages Paged for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


            return Request.CreateResponse<MessageModels.PagedResults>(HttpStatusCode.OK, new MessageModels.PagedResults()
            {
                TotalRecords = totalRecords,
                Results = messages.Select(m => new MessageModels.MessageResponse()
                {
                    amount = m.Amount,
                    comments = (!String.IsNullOrEmpty(m.Comments) ? String.Format("{0}", m.Comments) : "No comments"),
                    createDate = formattingServices.FormatDateTimeForJSON(m.CreateDate),
                    Id = m.Id,
                    //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                    messageStatus = (m.Direction == "In" ? m.Status.GetRecipientMessageStatus() : m.Status.GetSenderMessageStatus()),
                    messageType = m.MessageType.ToString(),
                    recipientUri = m.RecipientUri,
                    senderUri = m.SenderUri,
                    direction = m.Direction,
                    latitude = m.Latitude,
                    longitutde = m.Longitude,
                    senderName = m.SenderName,
                    transactionImageUri = m.TransactionImageUrl,
                    recipientName = m.RecipientName,
                    senderSeen = m.senderHasSeen,
                    recipientSeen = m.recipientHasSeen,
                    isAcceptable = IsAcceptable(m),
                    isRejectable = IsRejectable(m),
                    isCancellable = IsCancellable(m),
                    isRemindable = IsRemindable(m),
                    senderImageUri = (m.Sender != null ? (!String.IsNullOrEmpty(m.Sender.ImageUrl) ? m.Sender.ImageUrl : ConfigurationManager.AppSettings["DefaultAvatarImage"]) : ConfigurationManager.AppSettings["DefaultAvatarImage"]),
                    recipientImageUri = (m.Recipient != null ? (!String.IsNullOrEmpty(m.Recipient.ImageUrl) ? m.Recipient.ImageUrl : (!String.IsNullOrEmpty(m.recipientImageUri) ? m.recipientImageUri : ConfigurationManager.AppSettings["DefaultAvatarImage"]))
                        : (!String.IsNullOrEmpty(m.recipientImageUri) ? m.recipientImageUri : ConfigurationManager.AppSettings["DefaultAvatarImage"]))

                }).ToList()
            });

        }
        private bool IsAcceptable(Domain.Message message)
        {
            if (message.Direction == "In")
                return message.Status.IsAcceptable();

            return false;
        }
        private bool IsRejectable(Domain.Message message)
        {
            if (message.Direction == "In")
                return message.Status.IsRejectable();

            return false;
        }
        private bool IsCancellable(Domain.Message message)
        {
            if (message.PaymentRequest != null)
                return false;

            if (message.Direction == "In")
                return false;
            else
                return message.Status.IsCancellable();
        }
        private bool IsExpressable(Domain.Message message)
        {
            if (message.Payment == null)
                return false;

            if (message.Payment.IsExpressed)
                return false;

            return message.Status.IsExpressable();
        }


        private bool IsRemindable(Domain.Message message)
        {
            if (message.Direction == "Out")
                return message.Status.IsRemindable();

            return false;
        }
        // GET /api/{userId}/PayStreamMessages
        [HttpGet]
        public HttpResponseMessage Get(string userId)
        {
            var userPayStreamMessageServices = new DomainServices.UserPayStreamMessageServices();
            var messageServices = new DomainServices.MessageServices();

            List<Domain.Message> messages = null;
            
            try 
            {
                messages = userPayStreamMessageServices.GetPayStreamMessage(userId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting User Paystream Messages for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting User Paystream Messages for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting User Paystream Messages for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            
            return Request.CreateResponse<List<MessageModels.MessageResponse>>(HttpStatusCode.OK, messages.Select(m => new MessageModels.MessageResponse()
            {
                amount = m.Amount,
                comments = (!String.IsNullOrEmpty(m.Comments) ? String.Format("{0}", m.Comments) : "No comments"),
                createDate = m.CreateDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                Id = m.Id,
                //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                messageStatus = (m.Direction == "In" ? m.Status.GetRecipientMessageStatus() : m.Status.GetSenderMessageStatus()),
                messageType = m.MessageType.ToString(),
                recipientUri = m.RecipientUri,
                recipientUriType = messageServices.GetURIType(m.RecipientUri).ToString(),
                senderUri = m.SenderUri,
                direction = m.Direction,
                latitude = m.Latitude,
                longitutde = m.Longitude,
                senderName = m.SenderName,
                transactionImageUri = m.TransactionImageUrl,
                recipientName = m.RecipientName,
                senderSeen = m.senderHasSeen,
                recipientSeen = m.recipientHasSeen,
                isAcceptable = IsAcceptable(m),
                isRejectable = IsRejectable(m),
                isCancellable = IsCancellable(m),
                isRemindable = IsRemindable(m),
                senderImageUri = (m.Sender != null ? (!String.IsNullOrEmpty(m.Sender.ImageUrl) ? m.Sender.ImageUrl : ConfigurationManager.AppSettings["DefaultAvatarImage"]) : ConfigurationManager.AppSettings["DefaultAvatarImage"]),
                recipientImageUri = (m.Recipient != null ? (!String.IsNullOrEmpty(m.Recipient.ImageUrl) ? m.Recipient.ImageUrl : (!String.IsNullOrEmpty(m.recipientImageUri) ? m.recipientImageUri : ConfigurationManager.AppSettings["DefaultAvatarImage"]))
                    : (!String.IsNullOrEmpty(m.recipientImageUri) ? m.recipientImageUri : ConfigurationManager.AppSettings["DefaultAvatarImage"]))

            }).ToList());

        }

        // GET /api/{userId}/PayStreamMessages/{id}
        [HttpGet]
        public HttpResponseMessage Get(string userId, string id)
        {
            var userPayStreamMessageServices = new DomainServices.UserPayStreamMessageServices();
            var messageService = new DomainServices.MessageServices();
            Domain.Message message = null;

            try
            {
                message = userPayStreamMessageServices.GetPayStreamMessage(userId, id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting User Paystream Messages for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting User Paystream Messages for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting User Paystream Messages for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


            return Request.CreateResponse<MessageModels.MessageResponse>(HttpStatusCode.OK, new MessageModels.MessageResponse()
            {
                amount = message.Amount,
                comments = (!String.IsNullOrEmpty(message.Comments) ? String.Format("{0}", message.Comments) : "No comments"),
                createDate = message.CreateDate.ToString("YYYY-MM-DDTHH:mm:ss.sssZ"),
                Id = message.Id,
                //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                messageStatus = (message.Direction == "In" ? message.Status.GetRecipientMessageStatus() : message.Status.GetSenderMessageStatus()),
                messageType = message.MessageType.ToString(),
                recipientUri = message.RecipientUri,
                recipientUriType = messageService.GetURIType(message.RecipientUri).ToString(),
                senderUri = message.SenderUri,
                direction = message.Direction,
                latitude = message.Latitude,
                longitutde = message.Longitude,
                senderName = message.SenderName,
                transactionImageUri = message.TransactionImageUrl,
                recipientName = message.RecipientName,
                senderSeen = message.senderHasSeen,
                recipientSeen = message.recipientHasSeen,
                isAcceptable = IsAcceptable(message),
                isRejectable = IsRejectable(message),
                isCancellable = IsCancellable(message),
                isRemindable = IsRemindable(message),
                senderImageUri = (message.Sender != null ? (!String.IsNullOrEmpty(message.Sender.ImageUrl) ? message.Sender.ImageUrl : ConfigurationManager.AppSettings["DefaultAvatarImage"]) : ConfigurationManager.AppSettings["DefaultAvatarImage"]),
                recipientImageUri = (message.Recipient != null ? (!String.IsNullOrEmpty(message.Recipient.ImageUrl) ? message.Recipient.ImageUrl : (!String.IsNullOrEmpty(message.recipientImageUri) ? message.recipientImageUri : ConfigurationManager.AppSettings["DefaultAvatarImage"]))
                    : (!String.IsNullOrEmpty(message.recipientImageUri) ? message.recipientImageUri : ConfigurationManager.AppSettings["DefaultAvatarImage"])),
                    payment = (message.Payment != null ? new PaymentModels.PaymentResponse() {
                        Amount = message.Payment.Amount,
                        Comments = message.Payment.Comments,
                        CreateDate = _formattingServices.FormatDateTimeForJSON(message.Payment.CreateDate),
                        EstimatedDeliveryDate = _formattingServices.FormatDateTimeForJSON(message.Payment.EstimatedDeliveryDate),
                        ExpressDeliveryDate = _formattingServices.FormatDateTimeForJSON(message.Payment.ExpressDeliveryDate),
                        ExpressDeliveryFee = message.Payment.ExpressDeliveryFee,
                        HoldDays = message.Payment.HoldDays,
                        Id = message.Payment.Id.ToString(),
                        IsExpressed = message.Payment.IsExpressed,
                        LastUpdatedDate = _formattingServices.FormatDateTimeForJSON(message.Payment.LastUpdatedDate),
                        PaymentStatus = message.Payment.PaymentStatus.ToString(),
                        PaymentVerificationLevel = message.Payment.PaymentVerificationLevel.ToString(),
                        RecipientAccountId = (message.Payment.RecipientAccount != null ? message.Payment.RecipientAccountId.ToString() : ""),
                        ScheduledProcessingDate = _formattingServices.FormatDateTimeForJSON(message.Payment.ScheduledProcessingDate),
                        SenderAccountId = message.Payment.SenderAccountId.ToString()
                    } : null)
});
        }

    }
}
