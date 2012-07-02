using System;
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
        
        public int PaymentStatusValue { get; set; }
        public PaymentStatus PaymentStatus
        {
            get { return (PaymentStatus)PaymentStatusValue; }
            set { PaymentStatusValue = (int)value; }
        }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public virtual Collection<Transaction> Transactions { get; set; }

        public virtual Message Message { get; set; }
    }
}
