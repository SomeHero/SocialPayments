using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{

    public class PaystreamMessageServices: ServicesBase
    {
        private string _paystreamMessageUrl = "";

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
        public string SendMoney(string apiKey, string senderId, string recipientId, string senderUri, string senderAccountId)
        {
            SendMessage();

            return "";
        }
        public string RequestMoney()
        {
            SendMessage();

            return "";
        }
        public string SendDonation()
        {
            SendMessage();

            return "";
        }
        public string AcceptPledge()
        {
            SendMessage();

            return "";
        }
        private string SendMessage()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            //var json = js.Serialize(new
            //{
            //    apiKey = apiKey,
            //    senderId = senderId,
            //    recipientId = recipientId,
            //    senderUri = senderUri
            //});

            //string jsonResponse = Post(_paystreamMessageUrl, json);

            //return jsonResponse;

            return "";
        }
    }
}