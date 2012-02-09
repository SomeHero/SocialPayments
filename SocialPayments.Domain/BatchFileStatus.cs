using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum BatchFileStatus
    {
        Initiated = 0,
        Created = 1,
        Validated = 2,
        ReadyForTransfer = 3,
        Transfered = 4,
        Rejected = 5
    }
}
