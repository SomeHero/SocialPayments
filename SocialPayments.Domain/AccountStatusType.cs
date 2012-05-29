﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum AccountStatusType
    {
        Submitted = 0,
        PendingActivation = 1,
        Verified = 2,
        Disabled = 3,
        Deleted = 4,
        NeedsReVerification =5
    }
}
