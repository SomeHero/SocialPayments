using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class UserLoginAudit
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public bool Success { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
