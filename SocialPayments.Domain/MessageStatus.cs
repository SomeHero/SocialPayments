using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class MessageStatus
    {
        public int Id { get; set; }
        public string InternalName { get; set; }
        public string ExternalName { get; set; }
    }
}
