using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.External.Models;
using System.Net;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Data.Entity;

namespace SocialPayments.RestServices.External.Controllers
{
    public class UserMessagesController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/users/{id}/messages
        public HttpResponseMessage<List<MessageModels.MessageResponse>> Get(string id)
        {
            User user = GetUser(id);

            if (user == null)
                return new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.NotFound);

            var messages = _ctx.Messages
                .Include("Recipient")
                .Include("Sender")
                .Include("SenderAccount")
                .Where(m => m.RecipientId.Value.Equals(user.UserId) || m.SenderId.Equals(user.UserId))
                .OrderByDescending(m => m.CreateDate).ToList();


            return new HttpResponseMessage<List<MessageModels.MessageResponse>>(
                    messages.Select(m => new MessageModels.MessageResponse()
                    {
                        amount = m.Amount,
                        comments = m.Comments,
                        createDate = m.CreateDate,
                        Id = m.Id.ToString(),
                        lastUpdatedDate = m.LastUpdatedDate,
                        messageStatus = m.Status.ToString(),
                        messageType = m.MessageType.ToString(),
                        recipient = (m.Recipient != null ? new UserModels.UserResponse()
                        {
                            createDate = m.Recipient.CreateDate,
                            emailAddress = m.Recipient.EmailAddress,
                            LastLoggedIn = m.Recipient.LastLoggedIn,
                            MobileNumber = m.Recipient.MobileNumber,
                            userId = m.Recipient.UserId,
                            userName = m.Recipient.UserName,
                            UserStatus = m.Recipient.UserStatus.ToString()
                        } : null),
                        recipientUri = m.RecipientUri,
                        sender = new UserModels.UserResponse()
                        {
                            createDate = m.Sender.CreateDate,
                            emailAddress = m.Sender.EmailAddress,
                            LastLoggedIn = m.Sender.LastLoggedIn,
                            MobileNumber = m.Sender.MobileNumber,
                            userId = m.Sender.UserId,
                            userName = m.Sender.UserName,
                            UserStatus = m.Sender.UserStatus.ToString()
                        },
                        senderUri = m.SenderUri,
                        senderAccount = (m.SenderAccount != null ? new AccountModels.AccountResponse() {
                            AccountNumber = m.SenderAccount.AccountNumber,
                            AccountType = m.SenderAccount.AccountType.ToString(),
                            Id = m.SenderAccount.Id.ToString(),
                            NameOnAccount = m.SenderAccount.NameOnAccount,
                            RoutingNumber = m.SenderAccount.RoutingNumber,
                        } : null),
                    }).ToList(), HttpStatusCode.OK);
        }
        private User GetUser(string id)
        {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            var user = _ctx.Users.FirstOrDefault(u => u.UserId.Equals(userId));

            if (user == null)
                return null;

            return user;
        }
    }
}
