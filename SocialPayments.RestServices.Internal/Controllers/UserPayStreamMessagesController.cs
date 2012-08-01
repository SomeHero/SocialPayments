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
using SocialPayments.Domain.ExtensionMethods;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPayStreamMessagesController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static DomainServices.FormattingServices _formattingServices = new DomainServices.FormattingServices();

        // GET /api/{userId}/PayStreamMessages
        public HttpResponseMessage<List<MessageModels.MessageResponse>> Get(string userId)
        {
           using(var _ctx = new Context())
           {
                DomainServices.MessageServices messageServices = new DomainServices.MessageServices(_ctx);
                DomainServices.UserService userServices = new DomainServices.UserService(_ctx);
               
                var user = userServices.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.NotFound);
                    message.ReasonPhrase = String.Format("User {0} specified in the request not found.", userId);

                    return message;
                }

                user.LastViewedPaystream = System.DateTime.Now;
                userServices.UpdateUser(user);

                var messages = messageServices.GetMessages(user.UserId);

                var messageResponse = messages.Select(m => new MessageModels.MessageResponse() 
                {
                        amount = m.Amount,
                        comments = (!String.IsNullOrEmpty(m.Comments) ? String.Format("{0}", m.Comments) : "No comments"),
                        createDate = m.CreateDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                        Id = m.Id,
                        //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                        messageStatus = (m.Direction == "In" ? m.Status.GetRecipientMessageStatus() :   m.Status.GetSenderMessageStatus()),
                        messageType = m.MessageType.ToString(),
                        recipientUri = m.RecipientUri,
                        senderUri = m.SenderUri,
                        direction = m.Direction,
                        latitude = m.Latitude,
                        longitutde = m.Longitude,
                        senderName = m.SenderName,
                        transactionImageUri = m.TransactionImageUrl,
                        recipientName = (m.Recipient != null ? _formattingServices.FormatUserName(m.Recipient) : m.RecipientName)
                    }).ToList();

                return new HttpResponseMessage<List<MessageModels.MessageResponse>>(messageResponse, HttpStatusCode.OK);
            }
        }

        private User GetUser(string id)
        {
            Context _ctx = new Context();
            DomainServices.MessageServices messageServices = new DomainServices.MessageServices(_ctx);

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
