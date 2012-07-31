using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string NameOnAccount { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public int AccountTypeId { get; set; }
        public AccountType AccountType 
        {
            get { return (AccountType)AccountTypeId; }
            set { AccountTypeId = (int)AccountType; }
        }
        public double Amount { get; set; }
        public int TransactionStatusId { get; set; }
        public TransactionStatus Status
        {
            get { return (TransactionStatus) TransactionStatusId; }
            set { TransactionStatusId = (int) value; }
        }
        public string ACHTransactionId { get; set; }
        public int TransactionTypeId { get; set; }
        public TransactionType Type
        {
            get { return (TransactionType)TransactionTypeId; }
            set { TransactionTypeId = (int)value; }
        }
        public int StandardEntryClassValue { get; set; }
        public StandardEntryClass StandardEntryClass
        {
            get { return (StandardEntryClass)StandardEntryClassValue; }
            set { StandardEntryClassValue = (int)value; }
        }
        public int PaymentChannelTypeValue { get; set; }
        public PaymentChannelType PaymentChannelType
        {
            get { return (PaymentChannelType)PaymentChannelTypeValue; }
            set { PaymentChannelTypeValue = (int)value; }
        }
        public string IndividualIdentifier { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public Guid? TransactionBatchId { get; set; }
        [ForeignKey("TransactionBatchId")]
        public virtual TransactionBatch TransactionBatch { get; set; }

    }
}
