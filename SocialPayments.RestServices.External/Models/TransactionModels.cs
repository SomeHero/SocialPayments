using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.External.Models
{
    public class TransactionModels
    {
        public class TransactionResponse
        {
            public String transactionId { get; set; }
            public String messageId { get; set; }
            public AccountModels.AccountResponse transactedAccount { get; set; }
            public Double amount { get; set; }
            public ACHTransactionInformation achTransactionInformation { get; set; }
            public String transactionCategory { get; set; }
            public String transactionStatus { get; set; }
            public DateTime createDate { get; set; }
            public DateTime? lastUpdatedDate { get; set; }
        }
        public class ACHTransactionInformation
        {
            public String transactionId { get; set; }
            public String standardEntryClass { get; set; }
            public String paymentChannel { get; set; }
            public String transactionBatchId { get; set; }
            public String transactionType { get; set; }
            public DateTime? transactionDate { get; set; }
        }

    }
}