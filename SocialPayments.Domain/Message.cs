using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ApiKey { get; set; }
        [ForeignKey("ApiKey")]
        public virtual Application Application { get; set; }
        public String SenderUri { get; set; }
        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; }
        public String RecipientUri { get; set; }
        public Guid? RecipientId { get; set; }
        [ForeignKey("RecipientId")]
        public User Recipient { get; set; }
        public Guid? SenderAccountId { get; set; }
        [ForeignKey("SenderAccountId")]
        public PaymentAccount SenderAccount { get; set; }
        public Double Amount { get; set; }
        public String Comments { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int MessageTypeValue { get; set; }
        public MessageType MessageType
        {
            get { return (MessageType)MessageTypeValue; }
            set { MessageTypeValue = (int)value; }
        }
        public int MessageStatusValue { get; set; }
        public MessageStatus MessageStatus
        {
            get { return (MessageStatus)MessageStatusValue; }
            set { MessageStatusValue = (int)value; }
        }

        public virtual Collection<Transaction> Transactions { get; set; }
    }
}
