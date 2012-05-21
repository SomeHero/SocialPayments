using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class MobileNumberSignUpKey
    {
        [Key]
        public Guid SignUpKey { get; set; }
        public string MobileNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
