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
            public String senderUri { get; set; }
            public UserModels.UserResponse sender { get; set; }
            public String recipientUri { get; set; }
            public UserModels.UserResponse recipient { get; set; }
            public AccountModels.AccountResponse senderAccount { get; set; }
            public Double amount { get; set; }
            public String comments { get; set; }
            public String createDate { get; set; }
            public String lastUpdatedDate { get; set; }
            public String messageType { get; set; }
            public String messageStatus { get; set; }
        }
        public class SubmitMessageRequest {
            public string apiKey { get; set; }
            public String senderUri { get; set; }
            public String senderAccountId { get; set; }
            public String recipientUri { get; set; }
            public Double amount { get; set; }
            public String comments { get; set; }
            public String messageType { get; set; }
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