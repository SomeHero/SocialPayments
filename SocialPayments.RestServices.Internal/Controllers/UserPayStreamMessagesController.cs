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

        // GET /api/{userId}/PayStreamMessages
        public HttpResponseMessage<List<MessageModels.MessageResponse>> Get(string userId)
        {
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

            List<MessageModels.MessageResponse> messageResponse = null;

            try
            {

                messageResponse = messages.Select(m => new MessageModels.MessageResponse()
                {
                    amount = m.Amount,
                    comments = m.Comments,
                    createDate = m.CreateDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                    Id = m.Id,
                    //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                    messageStatus = m.MessageStatus.ToString(),
                    messageType = m.MessageType.ToString(),
                    recipientUri = m.RecipientUri,
                    senderUri = m.SenderUri
                }).ToList();
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
