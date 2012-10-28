using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum TransactionStatus
    {
        Pending = 1,   //created and waiting to be batched
        Complete = 2,  //settled with bank
        Failed = 3,    //unknown error occurred
        Returned = 4,  //transaction was returned
        Cancelled = 5,   //transaction was cancelled and will not be batchws
        Processed = 6    //sent to bank in a nacha file
    }
}
