using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class FBUser
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public String FBUserID { get; set; }
        public string OAuthToken { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
