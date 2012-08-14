using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class UserAttribute
    {
        public Guid Id { get; set; }
        public string AttributeName { get; set; }
        public bool Approved { get; set; }
        public bool IsActive { get; set; }
        public int Points { get; set; }
    }
}
