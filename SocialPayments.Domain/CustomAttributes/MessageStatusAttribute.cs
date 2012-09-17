using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.CustomAttributes
{
    public class MessageStatusAttribute: Attribute
    {
        public string SenderDescription { get; set; }
        public string RecipientDescription { get; set; }
    }
}
