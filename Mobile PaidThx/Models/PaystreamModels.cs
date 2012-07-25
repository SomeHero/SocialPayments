using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class PaystreamModels
    {
        public class PaystreamModel
        {
            public List<PaymentModel> AllReceipts { get; set; }
            public List<PaymentModel> PaymentReceipts { get; set; }
            public List<PaymentModel> RequestReceipts { get; set; }
            public List<PaymentModel> Alerts { get; set; }
            public ProfileModels ProfileModel { get; set; }
        }
        public class PaymentModel
        {
            public string Id { get; set; }
            public string SenderUri { get; set; }
            public string RecipientUri { get; set; }
            public Double Amount { get; set; }
            public DateTime TransactionDate { get; set; }
            public TransactionType TransactionType { get; set; }
            public TransactionStatus TransactionStatus { get; set; }
            public MessageType MessageType { get; set; }
            public string Direction { get; set; }
            public string Comments { get; set; }
            public string TransactionImageUri { get; set; }
        }
        public class AlertModel
        {
            public string AlertMessage { get; set; }
            public DateTime AlertDate { get; set; }
        }
    }
}