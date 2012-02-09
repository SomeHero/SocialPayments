using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class Application
    {
        public string ApplicationName { get; set; }
        public string Url { get; set; }
        [Key]
        public Guid ApiKey { get; set; }
        public bool IsActive { get; set; }
    }
}
