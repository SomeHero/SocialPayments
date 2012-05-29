using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class PayPoint
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string URI { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
