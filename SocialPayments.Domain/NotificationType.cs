using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class NotificationType
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
    }
}
