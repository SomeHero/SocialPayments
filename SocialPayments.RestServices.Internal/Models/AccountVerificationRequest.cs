using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class AccountVerificationRequest
    {
        public int depositAmount1 { get; set; }
        public int depositAmount2 { get; set; }
    }
}
