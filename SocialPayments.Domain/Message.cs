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
        public virtual User Sender { get; set; }
        public String RecipientUri { get; set; }
        public Guid? RecipientId { get; set; }
        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; }
        public Guid? SenderAccountId { get; set; }
        [ForeignKey("SenderAccountId")]
        public virtual PaymentAccount SenderAccount { get; set; }
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
        public int StatusValue { get; set; }
        public PaystreamMessageStatus Status
        {
            get { return (PaystreamMessageStatus)StatusValue; }
            set { StatusValue = (int)value; }
        }
        public int WorkflowStatusValue { get; set; }
        public PaystreamMessageWorkflowStatus WorkflowStatus
        {
            get { return (PaystreamMessageWorkflowStatus)WorkflowStatusValue; }
            set { WorkflowStatusValue = (int)value; }
        }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string senderFirstName { get; set; }
        public string senderLastName { get; set; }
        public string senderImageUri { get; set; }
        public string recipientFirstName { get; set; }
        public string recipientLastName { get; set; }
        public string recipientImageUri { get; set; }
        public string shortUrl { get; set; }

        public virtual Payment Payment { get; set; }

        public virtual Message PaymentRequest { get; set; }
        public virtual User Originator { get; set; }

        [NotMapped]
        public string Direction { get; set; }
        [NotMapped]
        public string SenderName { get; set; }
        [NotMapped]
        public string RecipientName { get; set; }
        [NotMapped]
        public string TransactionImageUrl { get; set; }

        public bool recipientHasSeen { get; set; }
        public bool senderHasSeen { get; set; }

        public double deliveryFeeAmount { get; set; }
        public int deliveryMethodId { get; set; }
        public DeliveryMethod deliveryMethod
        {
            get { return (DeliveryMethod)deliveryMethodId; }
            set { deliveryMethodId = (int)value; }
        }
    }

    public class SameRecipientComparer : IEqualityComparer<Message>
    {
        public bool Equals(Message one, Message two)
        {
            if (one.RecipientUri == two.RecipientUri)
                return true;
            else if (one.Recipient != null && two.Recipient != null && one.Recipient == two.Recipient)
                return true;
            else
                return false;
        }

        public int GetHashCode(Message msg)
        {
            // A hash value SHOULD be implemented here,
            // but we only want to know if message recipients are equal.
            // We return the hash code of the recipient Id of the messages.
            if (msg.Recipient == null || msg.RecipientId == null)
                return msg.RecipientUri.GetHashCode();
            else
                return msg.RecipientId.GetHashCode();
        }
    }

}
