using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class SystemAlerts
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatDate { get; set; }
    }
}
