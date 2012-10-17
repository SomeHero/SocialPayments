using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class PaystreamModels
    {
        public class PaystreamModel
        {
            public String UserId { get; set; }
            public ProfileModels ProfileModel { get; set; }
        }
        public class PaystreamDetailModel
        {
            public Guid Id { get; set; }
            public string senderUri { get; set; }
            public UserModels.UserResponse sender { get; set; }
            public string recipientUri { get; set; }
            public string recipientUriType { get; set; }
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
            public string senderImageUri { get; set; }
            public string recipientImageUri { get; set; }
            public string recipientName { get; set; }
            public double latitude { get; set; }
            public double longitutde { get; set; }
            public bool senderSeen { get; set; }
            public bool recipientSeen { get; set; }
            public bool isCancellable { get; set; }
            public bool isRemindable { get; set; }
            public bool isAcceptable { get; set; }
            public bool isRejectable { get; set; }
            public bool isExpressable { get; set; }

            //public PaymentModels.PaymentResponse Payment { get; set; }
        }
        public class PinSwipeRequestModel
        {
            public string PaystreamAction { get; set; }
            public string MessageId { get; set; }
            public MessageModels.MessageResponse Message { get; set; }
        }
        public class PinSwipeModel
        {
            public string Pincode { get; set; }
        }
        public class SendReminder
        {
            public string MessageId { get; set; }
            public string UriType { get; set; }
        }
        public class SendReminderPostModel
        {
            public string MessageId { get; set; }
            public string ReminderMessage { get; set; }
        }
    }
}