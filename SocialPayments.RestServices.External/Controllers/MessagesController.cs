using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.External.Models;
using System.Net;
using SocialPayments.DataLayer;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.External.Controllers
{
    public class MessagesController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/messages
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // GET /api/messages/5
        public HttpResponseMessage<MessageModels.MessageResponse> Get(string id)
        {
            var message = GetMessage(id);
            //TODO: check to see if message exists with id
            if (message == null)
                return new HttpResponseMessage<MessageModels.MessageResponse>(HttpStatusCode.NotFound);

            //TODO: return message 
            var response = new HttpResponseMessage<MessageModels.MessageResponse>(new MessageModels.MessageResponse()
            {
                amount = message.Amount,
                comments = message.Comments,
                createDate = message.CreateDate,
                Id = message.Id.ToString(),
                lastUpdatedDate = message.LastUpdatedDate,
                messageStatus = message.Status.ToString(),
                messageType = message.MessageType.ToString(),
                recipient = new UserModels.UserResponse()
                {

                },
                recipientUri = message.RecipientUri,
                sender = new UserModels.UserResponse()
                {

                },
                senderUri = message.SenderUri,
                senderAccount = new AccountModels.AccountResponse()
                {

                }
            }, HttpStatusCode.OK);

            return response;
        }
        
        // Get /api/message?date={date}
        public HttpResponseMessage<List<MessageModels.MessageResponse>> GetMessagesByDate(DateTime messageDate)
        {
            var messages = _ctx.Messages
                .Where(m => m.CreateDate.Date.Equals(messageDate.Date))
                .OrderByDescending(m => m.CreateDate)
                .ToList<Message>();

            var messageResponses = messages.Select(m => new MessageModels.MessageResponse()
            {
                amount = m.Amount,
                comments = m.Comments,
                createDate = m.CreateDate,
                Id = m.Id.ToString(),
                lastUpdatedDate = m.LastUpdatedDate,
                messageStatus = m.Status.ToString(),
                messageType = m.MessageType.ToString(),
                recipient = new UserModels.UserResponse()
                {

                },
                recipientUri = m.RecipientUri,
                sender = new UserModels.UserResponse()
                {

                },
                senderUri = m.SenderUri,
                senderAccount = new AccountModels.AccountResponse()
                {

                }
            }).ToList();

            var response = new HttpResponseMessage<List<MessageModels.MessageResponse>>(messageResponses, HttpStatusCode.OK);

            return response;
        }
        // POST /api/messages
        public HttpResponseMessage Post(MessageModels.SubmitMessageRequest request)
        {
            var sender = GetUser(request.senderUri);

            if (sender == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Sender {0} specified in the request not found.", request.senderUri);

                return message;
            }
            
            var senderAccount = GetAccount(sender, request.senderAccountId);

            if(senderAccount == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Sender Account {0} specified in the request is inactive or not owned by Sender {1}.", request.senderAccountId, request.senderUri);

                return message;
            }

            //TODO: validate application in request
            //var application = GetApplication(request.apiKey);

            if(application == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Application {0} specified in the request is invalid", request.apiKey);

                return message;
            }

            //TODO: confirm recipient is valid???

            //TODO: confirm amount is within payment limits

            //TODO: try to add message
            try
            {
                MessageType messageType = MessageType.Payment;
                
                if (request.messageType == "Payment")
                    messageType = MessageType.Payment;

                if (request.messageType == "PaymentRequest")
                    messageType = MessageType.PaymentRequest;

                _ctx.Messages.Add(new Message()
                {
                    Amount = request.amount,
                    Application = application,
                    ApiKey = application.ApiKey,
                    Comments = request.comments,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    Status = PaystreamMessageStatus.Processing,
                    WorkflowStatus = PaystreamMessageWorkflowStatus.Pending,
                    MessageType = messageType,
                    MessageTypeValue = (int)messageType,
                    RecipientUri = request.recipientUri,
                    SenderUri = request.senderUri,
                    Sender= sender,
                    SenderId = sender.UserId,
                    SenderAccount = senderAccount,
                    SenderAccountId = senderAccount.Id
                });

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = ex.Message;

                return message;
            }
            
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Created);
            //responseMessage.Headers.C
            return responseMessage;
        }

        private Application GetApplication(string id)
        {
            Guid applicationKey;

            Guid.TryParse(id, out applicationKey);

            if (applicationKey == null)
                return null;

            var application = _ctx.Applications.FirstOrDefault(a => a.ApiKey == applicationKey);

            return application;
        }

        private PaymentAccount GetAccount(User sender, string id)
        {
            Guid accountId;

            Guid.TryParse(id, out accountId);

            if(accountId == null)
                return null;

            foreach (var account in sender.PaymentAccounts)
            {
                if (account.Id == accountId)
                    return account;

            }

            return null;

        }

        private User GetUser(string mobileNumber)
        {

            var user = _ctx.Users.FirstOrDefault(u => u.MobileNumber.Equals(mobileNumber));

            return user;
        }

        // PUT /api/messages/5
        public HttpResponseMessage Put(int id, MessageModels.UpdateMessageRequest request)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/messages/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        private Message GetMessage(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            if (messageId == null)
                return null;

            var message = _ctx.Messages.FirstOrDefault(m => m.Id.Equals(messageId));

            return message;
        }
    }
}
