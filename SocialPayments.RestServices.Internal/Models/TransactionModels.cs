using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class TransactionModels
    {
        public class TransactionResponse
        {
            public Guid Id { get; set; }
            public AccountModels.AccountResponse PaymentAccount { get; set; }
            public double Amount { get; set; }
            public string Status { get; set; }
            public string ACHTransactionId { get; set; }
            public string Type { get; set; }
            public string Category { get; set; }
            public string StandardEntryClass { get; set; }
            public string PaymentChannelType { get; set; }
            public string CreateDate { get; set; }
            public string LastUpdatedDate { get; set; }
        }
    }
}