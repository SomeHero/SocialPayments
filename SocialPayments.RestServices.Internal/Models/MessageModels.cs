﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class MessageModels
    {
        public class PagedResults
        {
            public int TotalRecords { get; set; }
            public IEnumerable<MessageResponse> Results { get; set; }
        }
        public class MessageResponse 
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
            public string recipientName { get; set; }
            public double latitude { get; set; }
            public double longitutde { get; set; }
            public bool senderSeen { get; set; }
            public bool recipientSeen { get; set; }
            public DateTime estimatedDeliveryDate { get; set; }

            public bool isCancellable { get; set; }
            public bool isRemindable { get; set; }
            public bool isAcceptable { get; set; }
            public bool isRejectable { get; set; }
             public bool isExpressable { get; set; }

             public string senderImageUri { get; set; }
             public string recipientImageUri { get; set; }

             public PaymentModels.PaymentResponse payment { get; set; }
        }
        public class MessageSeenUpdateRequest
        {
            public string userId { get; set; }
            public List<string> messageIds { get; set; }
        }
        public class MultipleURIRequest
        {
            public List<string> recipientUris { get; set; }
        }

        public class MultipleURIResponse
        {
            public string userUri { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
        }

        public class SubmitMessageRequest {
            public string apiKey { get; set; }
            public string senderId { get; set; }
            public string recipientId { get; set; }
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
            public string deliveryMethod { get; set; }
        }
        public class SubmitDonateRequest
        {
            public string senderId { get; set; }
            public string organizationId { get; set; }
            public string senderAccountId { get; set; }
            public double amount { get; set; }
            public string comments { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string recipientFirstName { get; set; }
            public string recipientLastName { get; set; }
            public string recipientImageUri { get; set; }
            public string securityPin { get; set; }
        }
        public class SubmitPledgeRequest
        {
            public string senderId { get; set; }
            public string onBehalfOfId { get; set; }
            public string recipientUri { get; set; }
            public double amount { get; set; }
            public string comments { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string recipientFirstName { get; set; }
            public string recipientLastName { get; set; }
            public string recipientImageUri { get; set; }
            public string securityPin { get; set; }
        }
        public class SubmitMessageResponse
        {
            public bool isLockedOut { get; set; }
            public int numberOfPinCodeFailures { get; set; }
        }
        public class AcceptPledgeRequestModel
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
            public string paymentAccountId { get; set; }
        }
        public class AcceptPaymentRequestModel
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
            public string paymentAccountId { get; set; }
        }
        public class CancelPaymentRequestModel
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
        }
        public class CancelPaymentRequestRequestModel 
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
        }
        public class RejectPledgeRequestModel
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
        }
        public  class RejectPaymentRequestModel
        {
            public string userId { get; set; }
            public string securityPin { get; set; }
        }
        public class UpdateMessageRequest {
            public String Id { get; set; }
            public String senderUri { get; set; }
            public String senderAccountId { get; set; }
            public String recipientUri { get; set; }
            public Double amount { get; set; }
            public String comments { get; set; }
        }
        public class ExpressPaymentRequest
        {
            public string securityPin { get; set; }
            public string sendAccountId { get; set; }
        }
    }
}