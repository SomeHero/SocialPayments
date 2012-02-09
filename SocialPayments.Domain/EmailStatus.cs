using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum EmailStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2,
        Bounced = 3
    }
}
