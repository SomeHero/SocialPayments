using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain
{
    public class Application
    {
        public string ApplicationName { get; set; }
        public string Url { get; set; }
        [Key]
        public Guid ApiKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public virtual Collection<ApplicationConfiguration> ConfigurationValues { get; set; }
        public virtual Collection<SocialNetwork> SocialNetworks { get; set; }
    }
}
