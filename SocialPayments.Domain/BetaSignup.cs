using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class BetaSignup
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
