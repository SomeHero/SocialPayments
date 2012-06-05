﻿using System;
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
        public int MessageStatusValue { get; set; }
        public MessageStatus MessageStatus
        {
            get { return (MessageStatus)MessageStatusValue; }
            set { MessageStatusValue = (int)value; }
        }

        public virtual Collection<Transaction> Transactions { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string senderFirstName { get; set; }
        public string senderLastName { get; set; }
        public string senderImageUri { get; set; }
        public string recipientFirstName { get; set; }
        public string recipientLastName { get; set; }
        public string recipientImageUri { get; set; }
    }
}