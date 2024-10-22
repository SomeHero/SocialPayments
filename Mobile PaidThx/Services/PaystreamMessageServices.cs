﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Services
{

    public class PaystreamMessageServices : ServicesBase
    {
        private string _paystreamMessageUrl = "{0}PaystreamMessages";
        private string _donateMessageUrl = "{0}PaystreamMessages/donate";
        private string _pledgeMessageUrl = "{0}/PaystreamMessages/pledge";
        private string _cancelPaymentUrl = "{0}/paystreammessages/{1}/cancel_payment";
        private string _cancelRequestUrl = "{0}/paystreammessages/{1}/cancel_request";
        private string _acceptPaymentRequestUrl = "{0}/paystreammessages/{1}/accept_request";
        private string _rejectPaymentRequestUrl = "{0}/paystreammessages/{1}/reject_request";
        private string _acceptPledgeRequestUrl = "{0}/paystreammessages/{1}/accept_pledge";
        private string _rejectPledgeRequestUrl = "{0}/paystreammessages/{1}/reject_pledge";
        private string _messageServicesBaseUrl = "{0}/Users/{1}/PaystreamMessages/{2}";
        private string _sendReminderBaseUrl = "{0}/Users/{1}/PaystreamMessages/send_reminder";


        public void SendMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri, string deliveryMethod)
        {
            SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri, deliveryMethod);

        }
        public void RequestMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri,
            string deliveryMethod)
        {
            SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri, deliveryMethod);

        }
        public void SendDonation(string apiKey, string senderId, string recipientId, string senderAccountId, string securityPin, double amount, string comments, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            SendDonationMessage(apiKey, senderId, recipientId, senderAccountId, securityPin, amount, comments, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri);
        }
        public void AcceptPledge(string apiKey, string senderId, string onBehalfOfId, string recipientUri, double amount, string comments, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri, string securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey = apiKey,
                senderId = senderId, 
                onBehalfOfId = onBehalfOfId,
                recipientUri = recipientUri, 
                amount = amount, 
                comments = comments, 
                latitude = latitude,
                longitude = longitude, 
                recipientFirstName = recipientFirstName,
                recipientLastName = recipientLastName,
                recipientImageUri = recipientImageUri, 
                securityPin = securityPin
            });

            var response = Post(String.Format(_pledgeMessageUrl, _webServicesBaseUrl), json);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void CancelPayment(string apiKey, string messageId, string userId, string securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                userId = userId,
                securityPin = securityPin
            });
            var response = Post(String.Format(_cancelPaymentUrl, _webServicesBaseUrl, messageId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public MessageModels.MessageResponse GetMessage(string userId, string messageId)
        {
            var serviceUrl = String.Format(_messageServicesBaseUrl, _webServicesBaseUrl, userId, messageId);
            var js = new JavaScriptSerializer();

            var response = Get(serviceUrl);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<MessageModels.MessageResponse>(response.JsonResponse);
        }
        public void CancelRequest(string apiKey, string messageId, string userId, string securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new {
                userId = userId,
                securityPin = securityPin
            });
            var response = Post(String.Format(_cancelRequestUrl, _webServicesBaseUrl, messageId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void AcceptPaymentRequest(string apiKey, string userId, string securityPin, string paymentAccountId, string messageId)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new {
                userId = userId,
                securityPin = securityPin,
                paymentAccountId = paymentAccountId
            });

            var response = Post(String.Format(_acceptPaymentRequestUrl, _webServicesBaseUrl, messageId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void RejectPaymentRequest(string apiKey, string messageId, string userId, string securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new {
                userId = userId,
                securityPin = securityPin
            });
            var response = Post(String.Format(_rejectPaymentRequestUrl, _webServicesBaseUrl, messageId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void AcceptPledgeRequest(string apiKey, string userId, string securityPin, string paymentAccountId, string messageId)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                userId = userId,
                securityPin = securityPin,
                paymentAccountId = paymentAccountId
            });

            var response = Post(String.Format(_acceptPledgeRequestUrl, _webServicesBaseUrl, messageId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void RejectPledgeRequest(string apiKey, string messageId, string userId, string securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                userId = userId,
                securityPin = securityPin
            });
            var response = Post(String.Format(_rejectPledgeRequestUrl, _webServicesBaseUrl, messageId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        private void SendMessage(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri,
            string securityPin, double amount, string comments, string messageType, string latitude, string longitude,
            string recipientFirstName, string recipientLastName, string recipientImageUri, string deliveryMethod)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey = apiKey,
                senderId = senderId,
                recipientId = recipientId,
                senderUri = senderUri,
                senderAccountId = senderAccountId,
                recipientUri = recipientUri,
                securityPin = securityPin,
                amount = amount,
                comments = comments,
                messageType = messageType,
                latitude = latitude,
                longitude = longitude,
                recipientFirstName = recipientFirstName,
                recipientLastName = recipientLastName,
                recipientImageUri = recipientImageUri,
                deliveryMethod = deliveryMethod
            });

            var response = Post(String.Format(_paystreamMessageUrl, _webServicesBaseUrl), json);
           
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }

        private void SendDonationMessage(string apiKey, string senderId, string recipientId, string senderAccountId, string securityPin, double amount, string comments, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey = apiKey,
                senderId = senderId,
                organizationId = recipientId,
                senderAccountId = senderAccountId,
                securityPin = securityPin,
                amount = amount,
                comments = comments,
                latitude = latitude,
                longitude = longitude,
                recipientFirstName = recipientFirstName,
                recipientLastName = recipientLastName,
                recipientImageUri = recipientImageUri
            });

            var response = Post(String.Format(_donateMessageUrl, _webServicesBaseUrl), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void SendReminder(string userId, string messageId, string reminderMessage)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                messageId = messageId,
                reminderMessage = reminderMessage
            });

            var response = Post(String.Format(_sendReminderBaseUrl, _webServicesBaseUrl, userId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
    }
}