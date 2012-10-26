using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class Bank
    {
        public Guid Id { get; set; }
        public String BankName { get; set; }
        public String RoutingNumber { get; set; }
        public int NumberOfSettlementDays { get; set; }
        public bool Active { get; set; }
    }
}
