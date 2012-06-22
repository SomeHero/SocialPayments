using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class MessageModels
    {
        public class MessageResponse {
            public Guid Id { get; set; }
            public string senderUri { get; set; }
            public UserModels.UserResponse sender { get; set; }
            public string recipientUri { get; set; }
            public UserModels.UserResponse recipient { get; set; }
            public AccountModels.AccountResponse senderAccount { get; set; }
            public double amount { get; set; }
            public string comments { get; set; }
            public string createDate { get; set; }
            public string lastUpdatedDate { get; set; }
            public string messageType { get; set; }
            public string messageStatus { get; set; }
            public string direction { get; set; }
            public string senderName { get; set; }
            public string transactionImageUri { get; set; }
            public string recipientName { get; set; }
            public double latitude { get; set; }
            public double longitutde { get; set; }
        }
        public class SubmitMessageRequest {
            public string apiKey { get; set; }
            public string senderId { get; set; }
           // public string senderUri { get; set; }
            public string senderAccountId { get; set; }
            public string recipientUri { get; set; }
            public string securityPin { get; set; }
            public double amount { get; set; }
            public string comments { get; set; }
            public string messageType { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string recipientFirstName { get; set; }
            public string recipientLastName { get; set; }
            public string recipientImageUri { get; set; }
            
        }
        public class AcceptPaymentRequestModel
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
            public string paymentAccountId { get; set; }
        }

        public class UpdateMessageRequest {
            public String Id { get; set; }
            public String senderUri { get; set; }
            public String senderAccountId { get; set; }
            public String recipientUri { get; set; }
            public Double amount { get; set; }
            public String comments { get; set; }
        }
    }
}