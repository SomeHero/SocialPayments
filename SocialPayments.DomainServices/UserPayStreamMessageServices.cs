using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Data.Entity;

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
                    .Include("Sender")
                    .Include("PaymentRequest")
                    .FirstOrDefault(m => m.Id == messageGuid);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Found", messageId));

                user.LastViewedPaystream = System.DateTime.Now;
                userServices.UpdateUser(user);

                ctx.SaveChanges();

                URIType senderUriType = URIType.MECode;
                URIType recipientUriType = URIType.MECode;

                string senderName = "";
                string recipientName = "";

                    senderUriType = URIType.MECode;
                    recipientUriType = URIType.MECode;

                    senderUriType = userServices.GetURIType(message.SenderUri);
                    recipientUriType = userServices.GetURIType(message.RecipientUri);

                    if (!String.IsNullOrEmpty(message.Sender.FirstName) || !String.IsNullOrEmpty(message.Sender.LastName))
                        senderName = message.Sender.FirstName + " " + message.Sender.LastName;
                    else if (!String.IsNullOrEmpty(message.senderFirstName) || !String.IsNullOrEmpty(message.senderLastName))
                        senderName = message.senderFirstName + " " + message.Sender.LastName;
                    else
                        senderName = (senderUriType == URIType.MobileNumber ? formattingServices.FormatMobileNumber(message.SenderUri) : message.SenderUri);

                    if (message.Recipient != null && (!String.IsNullOrEmpty(message.Recipient.FirstName) || !String.IsNullOrEmpty(message.Recipient.LastName)))
                        recipientName = message.Recipient.FirstName + " " + message.Recipient.LastName;
                    else if (!String.IsNullOrEmpty(message.recipientFirstName) || !String.IsNullOrEmpty(message.recipientLastName))
                        recipientName = message.recipientFirstName + " " + message.recipientLastName;
                    else
                        recipientName = (recipientUriType == URIType.MobileNumber ? formattingServices.FormatMobileNumber(message.RecipientUri) : message.RecipientUri);

                    message.SenderName = senderName;
                    message.RecipientName = recipientName;
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
    }
}
