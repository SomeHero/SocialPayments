using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum FeeStatus
    {
        Processing = 1,
        Complete = 2,
        Returned = 3,
        Refunded = 4
    }
}
