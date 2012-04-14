using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class FBUser
    {
        public Guid Id { get; set; }
        public String FBUserID { get; set; }
        public string OAuthToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public virtual User User { get; set; }
    }
}
