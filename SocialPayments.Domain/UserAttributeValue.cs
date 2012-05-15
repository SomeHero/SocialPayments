using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class UserAttributeValue
    {
        public Guid id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public Guid UserAttributeId { get; set; }
        [ForeignKey("UserAttributeId")]
        public UserAttribute UserAttribute { get; set; }
        public string AttributeValue { get; set; }
    }
}
