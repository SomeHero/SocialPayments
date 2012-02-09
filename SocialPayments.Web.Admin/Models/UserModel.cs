using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.Web.Admin.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string MobileNumber { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? SignUpDate { get; set; }
    }
}