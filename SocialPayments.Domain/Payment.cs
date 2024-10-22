﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid ApiKey { get; set; }
        [ForeignKey("ApiKey")]
        public virtual Application Application { get; set; }

        public Guid SenderAccountId { get; set; }
        [ForeignKey("SenderAccountId")]
        public virtual PaymentAccount SenderAccount { get; set; }

        public Guid? RecipientAccountId { get; set; }
        [ForeignKey("RecipientAccountId")]
        public virtual PaymentAccount RecipientAccount { get; set; }

        public double Amount { get; set; }
        public string Comments { get; set; }
        public int HoldDays { get; set; }
        public DateTime ScheduledProcessingDate { get; set; }

        public int PaymentStatusValue { get; set; }
        public PaymentStatus PaymentStatus
        {
            get { return (PaymentStatus)PaymentStatusValue; }
            set { PaymentStatusValue = (int)value; }
        }
        public int PaymentVerificationLevelValue { get; set; }
        public PaymentVerificationLevel PaymentVerificationLevel
        {
            get { return (PaymentVerificationLevel)PaymentVerificationLevelValue; }
            set { PaymentVerificationLevelValue = (int)value; }
        }

        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public virtual Message Message { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        //Delivery
        public DateTime EstimatedDeliveryDate { get; set; }
        public bool IsExpressed { get; set; }
        public double ExpressDeliveryFee { get; set; }
        public DateTime ExpressDeliveryDate { get; set; }

        //Fees
        public virtual List<Fee> Fees { get; set; }

    }
}
