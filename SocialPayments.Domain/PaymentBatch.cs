using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class PaymentBatch
    {
        public int Id { get; set; }
        public DateTime BatchOpenTime { get; set; }
        public DateTime BatchCloseTime { get; set; }
        public List<Payment> Payments { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DateLastUpdated { get; set; }
    }
}
