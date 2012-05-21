using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface ITransaction
    {
        Guid Id { get; set; }
        Guid MessageId { get; set; }
        IMessage Message { get; set; }
        Guid FromAccountId { get; set; }
        IPaymentAccount FromAccount { get; set; }
        double Amount { get; set; }
        int TransactionStatusId { get; set; }
        TransactionStatus Status { get; set; }
        string ACHTransactionId { get; set; }
        int TransactionTypeId { get; set; }
        TransactionType Type { get; set; }
        int TransactionCategoryId { get; set; }
        TransactionCategory Category { get; set; }
        int StandardEntryClassValue { get; set; }
        StandardEntryClass StandardEntryClass { get; set; }
        int PaymentChannelTypeValue { get; set; }
        PaymentChannelType PaymentChannelType { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? SentDate { get; set; }
        DateTime? LastUpdatedDate { get; set; }
        Guid TransactionBatchId { get; set; }
        ITransactionBatch TransactionBatch { get; set; }
        Guid UserId { get; set; }
        IUser User { get; set; }
    }
}
