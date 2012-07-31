using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaystreamMessageStatus
    {
        Submitted = 0,
        Processing = 1,
        Accepted = 2,
        Rejected = 3,
        Cancelled = 4,
        Complete = 5,
        Returned = 6
    }
}
