using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class PaymentRequest
    {
        public Guid ApiKey { get; set; }
        [ForeignKey("ApiKey")]
        public virtual Application Application { get; set; }
        public Guid PaymentRequestId { get; set; }
        public Guid RequestorId { get; set; }
        [ForeignKey("RequestorId")]
        public virtual User Requestor { get; set; }
        [Required]
        public String RecipientUri { get; set; }
        public Guid? RecipientId { get; set; }
        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; }
        [Required]
        public Double Amount { get; set; }
        public String Comments { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int PaymentRequestStatusValue { get; set; }
        public PaymentRequestStatus PaymentRequestStatus
        {
            get { return (PaymentRequestStatus)PaymentRequestStatusValue; }
            set { PaymentRequestStatusValue = (int)value; }
        }

        public DateTime? LastReminderSent { get; set; }
    }
}
