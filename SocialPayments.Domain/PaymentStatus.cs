using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaymentStatus
    {
        Submitted = 1,  //created
        Pending = 2,    //waiting to be batched
        Complete = 3,   //settled with bank
        Returned = 4,   //payment was returned 
        Cancelled = 5,  //payment was cancelled before batched
        Refunded = 6,   //payment was refunded because recipient never claimed payment after X days
        Open = 7,       //withdrawal transaction was batched but payment has not been picked up
        Processed = 8  //sent to bank in nacha file
    }
}
