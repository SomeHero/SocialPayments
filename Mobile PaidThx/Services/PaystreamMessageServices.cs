﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{

    public class PaystreamMessageServices : ServicesBase
    {
        private string _paystreamMessageUrl = "http://23.21.203.171/api/internal/api/PaystreamMessages";

        //public string apiKey { get; set; }
        //public string senderId { get; set; }
        //public string recipientId { get; set; }
        //// public string senderUri { get; set; }
        //public string senderAccountId { get; set; }
        //public string recipientUri { get; set; }
        //public string securityPin { get; set; }
        //public double amount { get; set; }
        //public string comments { get; set; }
        //public string messageType { get; set; }
        //public double latitude { get; set; }
        //public double longitude { get; set; }
        //public string recipientFirstName { get; set; }
        //public string recipientLastName { get; set; }
        //public string recipientImageUri { get; set; }
        public string SendMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            return SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri);

        }
        public string RequestMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            return SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri);

        }
        public string SendDonation(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            return SendMessage(apiKey, senderId, recipientId, senderUri, senderAccountId, recipientUri, securityPin, amount, comments, messageType, latitude, longitude, recipientFirstName, recipientLastName, recipientImageUri);
        }
        public string AcceptPledge()
        {
            //SendMessage();

            return "";
        }
        private string SendMessage(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId, string recipientUri, string securityPin, double amount, string comments, string messageType, string latitude, string longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
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

            string jsonResponse = Post(_paystreamMessageUrl, json);

            return jsonResponse;
        }
    }
}