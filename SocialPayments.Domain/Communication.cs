using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class Communication
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public CommunicationMethod Method { get; set; }
        public CommunicationType Type { get; set; }
        public string Template { get; set; }
    }
}
