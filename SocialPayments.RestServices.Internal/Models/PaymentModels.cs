using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class PaymentModels
    {
        public class PaymentResponse
        {
            public string Id { get; set; }
            public string SenderAccountId { get; set; }
            public string RecipientAccountId { get; set; }

            public double Amount { get; set; }
            public string Comments { get; set; }
            public int HoldDays { get; set; }
            public string ScheduledProcessingDate { get; set; }
            public string PaymentStatus { get; set; }
            public string PaymentVerificationLevel { get; set; }
            public string CreateDate { get; set; }
            public string LastUpdatedDate { get; set; }

            //Delivery
            public string EstimatedDeliveryDate { get; set; }
            public bool IsExpressed { get; set; }
            public double ExpressDeliveryFee { get; set; }
            public string ExpressDeliveryDate { get; set; }

            //Fees
            //public virtual List<Fee> Fees { get; set; }
        }
    }
}