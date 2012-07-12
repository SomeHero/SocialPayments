using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class SocialNetwork
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
