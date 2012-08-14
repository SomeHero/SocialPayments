using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaystreamMessageWorkflowStatus
    {
        Pending = 1,
        Complete = 2,
        Failed = 3
    }
}
