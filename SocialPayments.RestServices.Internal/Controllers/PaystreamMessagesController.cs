﻿using System;
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

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class PaystreamMessagesController : ApiController
    {
        private Context _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DomainServices.ValidationService _validationService = new DomainServices.ValidationService(_logger);
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
            
            User sender = null;

            try
            {
                sender = GetUser(request.senderUri);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Debug, String.Format("Exception getting Sender {0}. {1}", request.senderUri, ex.Message));
            }

            if (sender == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Sender {0} specified in the request not found.", request.senderUri);

                return message;
            }
            if (_validationService.AreMobileNumbersEqual(sender.MobileNumber,  request.recipientUri))
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Specified sender {0} is the same as the recipient {1}.", request.senderUri, request.recipientUri);

                return message;
            }

            PaymentAccount senderAccount = null;

            try
            {
                senderAccount = GetAccount(sender, request.senderAccountId);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Debug, String.Format("Exception getting Sender Account {0}. {1}", request.senderAccountId, ex.Message));
            }

            if (senderAccount == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Sender Account {0} specified in the request is inactive or not owned by Sender {1}.", request.senderAccountId, request.senderUri);

                return message;
            }

            //TODO: validate application in request
            Application application = null;

            try
            {
                application = GetApplication(request.apiKey);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Debug, String.Format("Exception getting application {0}. {1}", request.apiKey, ex.Message));
            }

            if (application == null)
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
                MessageStatus messageStatus = MessageStatus.Submitted;
                if (request.messageType == "Payment")
                    messageType = MessageType.Payment;

                if (request.messageType == "PaymentRequest")
                    messageType = MessageType.PaymentRequest;

               var message = _ctx.Messages.Add(new Message()
                {
                    Amount = request.amount,
                    Application = application,
                    ApiKey = application.ApiKey,
                    Comments = request.comments,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    MessageStatus = MessageStatus.Pending,
                    MessageStatusValue = (int)messageStatus,
                    MessageType = messageType,
                    MessageTypeValue = (int)messageType,
                    RecipientUri = _formattingService.FormatMobileNumber(request.recipientUri),
                    SenderUri = _formattingService.FormatMobileNumber(request.senderUri),
                    Sender = sender,
                    SenderId = sender.UserId,
                    SenderAccount = senderAccount,
                    SenderAccountId = senderAccount.Id
                });

                _ctx.SaveChanges();

                AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

                client.Publish(new PublishRequest()
                {
                    Message = message.Id.ToString(),
                    TopicArn = ConfigurationManager.AppSettings["MessagePostedTopicARN"],
                    Subject = "New Message Receivied"
                });
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

        // PUT /api/paystreammessage/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/paystreammessage/5
        public void Delete(int id)
        {
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

            if (accountId == null)
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

            var user = _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(u => u.MobileNumber.Equals(mobileNumber));

            return user;
        }
    }
}
