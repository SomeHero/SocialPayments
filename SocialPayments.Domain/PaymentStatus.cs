using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaymentStatus
    {
        Submitted = 1,
        Pending = 2,
        Complete = 3,
        Returned = 4,
        Cancelled = 5,
        Refunded = 6,
        Open = 7
    }
}
