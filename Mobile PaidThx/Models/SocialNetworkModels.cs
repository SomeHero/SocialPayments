using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class SocialNetworksModels
    {
        public class Index
        {
            public string FBState { get; set; }
            public List<UserSocialNetwork> UserSocialNetworks { get; set; }
        }
        public class UserSocialNetwork {
            public string Name { get; set; }
        }
    }
}