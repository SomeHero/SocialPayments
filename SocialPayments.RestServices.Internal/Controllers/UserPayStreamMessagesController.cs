using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using NLog;
using System.Net;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPayStreamMessagesController : ApiController
    {
        private Context _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static DomainServices.FormattingServices _formattingServices = new DomainServices.FormattingServices();

        // GET /api/{userId}/PayStreamMessages
        public HttpResponseMessage<List<MessageModels.MessageResponse>> Get(string userId)
        {
            DomainServices.MessageServices messageServices = new DomainServices.MessageServices(_ctx);

            var user = GetUser(userId);

            if (user == null)
            {
                var message = new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.NotFound);
                message.ReasonPhrase = String.Format("User {0} specified in the request not found.", userId);

                return message;
            }

            var messages = _ctx.Messages
                .Where(m => m.SenderId == user.UserId || m.RecipientId.Value == user.UserId)
                .OrderByDescending(m => m.CreateDate)
                .ToList<Message>();

            List<MessageModels.MessageResponse> messageResponse = new List<MessageModels.MessageResponse>();

            URIType senderUriType = URIType.MECode;
            URIType recipientUriType = URIType.MECode;

            try
            {
                foreach (var message in messages)
                {
                    senderUriType = URIType.MECode;
                    recipientUriType = URIType.MECode;
                    
                    senderUriType = messageServices.GetURIType(message.SenderUri);
                    recipientUriType = messageServices.GetURIType(message.RecipientUri);

                    _logger.Log(LogLevel.Error, String.Format("URI Formats {0} {1}", senderUriType, recipientUriType));

                    String direction = "In";

                    if (message.SenderId.Equals(user.UserId))
                        direction = "Out";

                    messageResponse.Add(new MessageModels.MessageResponse()
                    {
                        amount = message.Amount,
                        comments = message.Comments,
                        createDate = message.CreateDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                        Id = message.Id,
                        //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                        messageStatus = message.MessageStatus.ToString(),
                        messageType = message.MessageType.ToString(),
                        recipientUri = (recipientUriType == URIType.MobileNumber ? _formattingServices.FormatMobileNumber(message.RecipientUri) : message.RecipientUri),
                        senderUri = (senderUriType == URIType.MobileNumber ? _formattingServices.FormatMobileNumber(message.SenderUri) : message.SenderUri),
                        direction = direction
                    });

                    _logger.Log(LogLevel.Error, String.Format("Added MEssage {0} {1}", senderUriType, recipientUriType));

                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception formatting response. {0}", ex.Message));
            }

            if (messageResponse == null)
            {
                var message = new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Unhandled exception get message for {0}", userId);

                return message;
            }

            return new HttpResponseMessage<List<MessageModels.MessageResponse>>(messageResponse, HttpStatusCode.OK);
        }

        private User GetUser(string id)
        {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            User user = null;

            try
            {
                user = _ctx.Users
                 .FirstOrDefault(u => u.UserId.Equals(userId));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception getting user {0}. {1}", id, ex.Message));

                var innerException = ex.InnerException;

                while (innerException != null)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception getting user {0}. {1}", id, innerException.Message));

                    innerException = innerException.InnerException;
                }
                return null;
            }

            return user;
        }

        // GET /api/{userId}/PayStreamMessages/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/{userId}/PayStreamMessages
        public void Post(string value)
        {
        }

        // PUT /api/userpaystreammessages/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/userpaystreammessages/5
        public void Delete(int id)
        {
        }
    }
}
