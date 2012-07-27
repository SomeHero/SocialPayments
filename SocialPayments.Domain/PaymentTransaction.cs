using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class PaymentTransaction
    {
        public Guid PaymentId { get; set; }
        [ForeignKey("PaymentId"), Column(Order = 0)]
        public Payment Payment { get; set; }

        [Key(), Column(Order = 1)]
        public Guid TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }
    }
}
