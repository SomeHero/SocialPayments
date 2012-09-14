﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{

    public class PaystreamMessageServices : ServicesBase
    {
        private string _paystreamMessageUrl = "{0}PaystreamMessages";
        private string _donateMessageUrl = "{0}PaystreamMessages/donate";
        private string _pledgeMessageUrl = "{0}/PaystreamMessages/accept_pledge";
        private string _cancelPaymentUrl = "/{0}/paystreammessages/{id}/cancel_payment";
        private string _cancelRequestUrl = "/{0}/paystreammessages/{id}/cancel_request";
        private string _acceptPaymentRequestUrl = "/{0}/paystreammessages/{id}/accept_request";
        private string _rejectPaymentRequestUrl = "/{0}/paystreammessages/{id}/reject_request";


        public void SendMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri);

        }
        public void RequestMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri);

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
                throw new Exception(response.Description);
        }
        public void CancelPayment(string apiKey, string messageId)
        {
            var response = Post(String.Format(_cancelPaymentUrl, _webServicesBaseUrl), "");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        private void SendMessage(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
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
                recipientImageUri = recipientImageUri
            });

            var response = Post(String.Format(_paystreamMessageUrl, _webServicesBaseUrl), json);
           
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception(response.Description);
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

            if (response.StatusCode != System.Net.HttpStatusCode.OK  && response.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception(response.Description);
        }
    }
}