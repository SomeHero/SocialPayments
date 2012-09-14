using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.BatchFileServices.NachaBatchFile.Models
{
    public class TransactionBatch
    {
        public Guid Id { get; set; }
        public int TotalNumberOfDeposits { get; set; }
        public int TotalNumberOfWithdrawals { get; set; }
        public double TotalDepositAmount { get; set; }
        public double TotalWithdrawalAmount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastDateUpdated { get; set; }
        public bool IsClosed { get; set; }
    }
    public class Transaction
    {
        public Guid Id { get; set; }
        public string NameOnAccount { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountType { get; set; }
        public double Amount { get; set; }
        public int TransactionStatusId { get; set; }
        public string Status { get; set; }
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
        public int PaymentId { get; set; }
    }
}
