using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum DonateRequestStatus
    {
        Pending = 1,
        Expired = 2,
        Complete = 3
    }
}
