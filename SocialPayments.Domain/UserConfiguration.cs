using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class UserConfiguration
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [MaxLength(50)]
        public string ConfigurationKey { get; set; }
        [MaxLength(50)]
        public string ConfigurationValue { get; set; }
        [MaxLength(100)]
        public string ConfigurationType { get; set; }
    }
}
