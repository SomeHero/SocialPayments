using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Data.Entity;
using System.Configuration;

namespace SocialPayments.DomainServices
{
    public class UserPayStreamMessageServices
    {
        public List<Domain.Message> GetPayStreamMessage(string userId)
        {
            using (var ctx = new Context())
            {
                DomainServices.MessageServices messageServices = new DomainServices.MessageServices();
                DomainServices.UserService userServices = new DomainServices.UserService(ctx);

                var user = userServices.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                user.LastViewedPaystream = System.DateTime.Now;
                userServices.UpdateUser(user);

                ctx.SaveChanges();

                return messageServices.GetMessages(user.UserId);


            }
        }
        public Domain.Message GetPayStreamMessage(string userId, string messageId)
        {
            using (var ctx = new Context())
            {
                DomainServices.MessageServices messageServices = new DomainServices.MessageServices();
                DomainServices.UserService userServices = new DomainServices.UserService(ctx);
                DomainServices.ValidationService validationServices = new DomainServices.ValidationService();
                DomainServices.FormattingServices formattingServices = new DomainServices.FormattingServices();

                var user = userServices.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                Guid messageGuid;
                Guid.TryParse(messageId, out messageGuid);

                if (messageGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Found", messageId));

                var message = ctx.Messages
                    .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Payment")
                    .Include("Payment.RecipientAccount")
                    .Include("Payment.SenderAccount")
                    .Include("PaymentRequest")
                    .FirstOrDefault(m => m.Id == messageGuid);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Found", messageId));

                user.LastViewedPaystream = System.DateTime.Now;
                userServices.UpdateUser(user);

                ctx.SaveChanges();

                message.SenderName = userServices.GetSenderName(message.Sender);
                if (message.Recipient != null)
                    message.RecipientName = userServices.GetSenderName(message.Recipient);
                else
                {
                    if (validationServices.IsPhoneNumber(message.RecipientUri))
                    {
                        message.RecipientUri = formattingServices.FormatMobileNumber(message.RecipientUri);
                        message.RecipientName = message.RecipientUri;
                    }
                    else if (validationServices.IsFacebookAccount(message.RecipientUri))
                        message.RecipientName = message.recipientFirstName + " " + message.recipientLastName;
                    else
                        message.RecipientName = message.RecipientUri;  
                }

                message.Direction = "In";

                if (message.SenderId == user.UserId)
                    message.Direction = "Out";


                if (message.Direction == "In")
                {
                    if (message.Sender != null && !String.IsNullOrEmpty(message.Sender.ImageUrl))
                        message.TransactionImageUrl = message.Sender.ImageUrl;
                }
                else
                {
                    if (message.Recipient != null && !String.IsNullOrEmpty(message.Recipient.ImageUrl))
                        message.TransactionImageUrl = message.Recipient.ImageUrl;
                    else
                        message.TransactionImageUrl = message.recipientImageUri;
                }
                
                return message;

            }
        }
        public void SendReminder(string userId, string messageId, string reminderMessage)
        {
            using (var ctx = new Context())
            {
                var userServices = new DomainServices.UserService(ctx);
                var validationServices = new DomainServices.ValidationService();
                var formattingServices = new DomainServices.FormattingServices();
                var emailService = new DomainServices.EmailService();
                var smsService = new DomainServices.SMSService();
                var facebookServices = new DomainServices.FacebookServices();

                
                var message = GetPayStreamMessage(userId, messageId);

                var recipientUriType = userServices.GetURIType(message.RecipientUri);
                var fromAddress = ConfigurationManager.AppSettings["fromEmailAddress"];
                var senderName = userServices.GetSenderName(message.Sender);

                switch (recipientUriType)
                {
                    case URIType.EmailAddress:
                        
                        emailService.SendEmail(message.ApiKey, fromAddress, message.RecipientUri, String.Format("Reminder from {0}", senderName, reminderMessage), reminderMessage);

                        break;

                    case URIType.MobileNumber:
                        smsService.SendSMS(message.ApiKey, message.RecipientUri, reminderMessage);

                        break;
                    case URIType.MECode:

                        var meCode = ctx.UserPayPoints.FirstOrDefault(p => p.URI == message.RecipientUri);

                        if (meCode == null)
                            throw new CustomExceptions.BadRequestException(String.Format("Unable to find MeCode {0}", message.RecipientUri));

                        if (!String.IsNullOrEmpty(meCode.User.EmailAddress))
                        {
                            emailService.SendEmail(message.ApiKey, fromAddress, message.RecipientUri, String.Format("Reminder from {0}", senderName, reminderMessage), reminderMessage);
                        }
                        if (!String.IsNullOrEmpty(meCode.User.MobileNumber))
                        {
                            smsService.SendSMS(message.ApiKey, message.RecipientUri, reminderMessage);
                        }


                        break;
                    case URIType.FacebookAccount:
                        facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.RecipientUri, reminderMessage, "");
                            
                        break;
                }
            }
        }
    }
}
