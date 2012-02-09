using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaymentRequestStatus
    {
        Submitted = 1,
        Pending = 2,
        Complete = 3,
        Expired = 4,
        Cancelled = 5
    }
}
