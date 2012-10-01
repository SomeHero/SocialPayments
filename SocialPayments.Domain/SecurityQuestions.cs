using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class SecurityQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public bool IsActive { get; set; }
    }
}
