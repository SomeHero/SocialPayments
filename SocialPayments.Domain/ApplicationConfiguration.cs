using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class ApplicationConfiguration
    {
        public Guid Id { get; set; }
        public Guid ApiKey { get; set; }
        [ForeignKey("ApiKey")]
        public Application Application { get; set; }
        [MaxLength(50)]
        public string ConfigurationKey { get; set; }
        [MaxLength(50)]
        public string ConfigurationValue { get; set; }
        [MaxLength(100)]
        public string ConfigurationType { get; set; }
    }
}
