using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class MobileDeviceAlias
    {
        public Guid Id { get; set; }
        public string MobileNumber { get; set; }
        public string MobileNumberAlias { get; set; }
    }
}
