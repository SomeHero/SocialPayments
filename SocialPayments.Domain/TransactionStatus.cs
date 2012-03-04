using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum TransactionStatus
    {
        Submitted = 0,
        Pending = 1,
        Complete = 2,
        Failed = 3,
        Returned = 4,
        Cancelled = 5
    }
}
