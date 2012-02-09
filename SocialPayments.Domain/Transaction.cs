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
        public Guid PaymentId { get; set; }
        public Guid FromAccountId { get; set; }
        [ForeignKey("FromAccountId")]
        public virtual PaymentAccount FromAccount
        {
            get;
            set;
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
        public int TransactionCategoryId { get; set; }
        public TransactionCategory Category
        {
            get { return (TransactionCategory)TransactionCategoryId; }
            set { TransactionCategoryId = (int)value; }
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
        public DateTime CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public Guid TransactionBatchId { get; set; }

        [ForeignKey("TransactionBatchId")]
        public virtual TransactionBatch TransactionBatch { get; set; }

    }
}
