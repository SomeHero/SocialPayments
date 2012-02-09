using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.Web.Admin.Models
{
    public class MobileDeviceAliasModel
    {
        public Guid Id { get; set; }
        public string MobileNumber { get; set; }
        public string MobileNumberAlias { get; set; }
    }
}