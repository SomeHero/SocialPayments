using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaymentWorkflowStatus
    {
        Pending = 1,
        Failed = 2,
        Complete = 3
    }
}
