using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain.Interfaces
{
    public interface IMessage
    {
        Guid Id { get; set; }
        Guid ApiKey { get; set; }
        IApplication Application { get; set; }
        String SenderUri { get; set; }
        Guid SenderId { get; set; }
        IUser Sender { get; set; }
        String RecipientUri { get; set; }
        Guid? RecipientId { get; set; }
        IUser Recipient { get; set; }
        Guid? SenderAccountId { get; set; }
        IPaymentAccount SenderAccount { get; set; }
        Double Amount { get; set; }
        String Comments { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? LastUpdatedDate { get; set; }
        int MessageTypeValue { get; set; }
        MessageType MessageType { get; set; }
        int MessageStatusValue { get; set; }
        MessageStatus MessageStatus { get; set; }
        Collection<ITransaction> Transactions { get; set; }
    }
}
