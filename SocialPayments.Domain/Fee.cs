using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class Fee
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public double FeeAmount { get; set; }
        public string FeeDescription { get; set; }
        public Guid SendAccountId { get; set; }
        public Guid ReceiveAccountId { get; set; }
        public Guid TransactionId { get; set; }
        public FeeStatus FeeStatus { get; set; }
    }
}
