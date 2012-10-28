using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Jobs.ProcessSettlementProcessor.Services.ResponseModels
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string NameOnAccount { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public int AccountTypeId { get; set; }
        public String AccountType { get; set; }
        public double Amount { get; set; }
        public String Status { get; set; }
        public string ACHTransactionId { get; set; }
        public int TransactionTypeId { get; set; }
        public string Type { get; set; }
        public int StandardEntryClassValue { get; set; }
        public string StandardEntryClass { get; set; }
        public int PaymentChannelTypeValue { get; set; }
        public string PaymentChannelType { get; set; }
        public string IndividualIdentifier { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public Guid? TransactionBatchId { get; set; }
        public Guid? PaymentId { get; set; }
    }
}
