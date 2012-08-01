using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SocialPayments.Domain
{
    public enum AccountStatusType
    {
        [Description("Pending")]
        Submitted = 0,
        [Description("Pending Activation")]
        PendingActivation = 1,
        [Description("Verified")]
        Verified = 2,
        [Description("Disabled")]
        Disabled = 3,
        [Description("Deleted")]
        Deleted = 4,
        [Description("Pending Activation")]
        NeedsReVerification =5
    }
}
