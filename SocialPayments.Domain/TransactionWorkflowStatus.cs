using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum TransactionWorkflowStatus
    {
        Pending = 1,
        Complete = 2,
        Returned = 3
    }
}
