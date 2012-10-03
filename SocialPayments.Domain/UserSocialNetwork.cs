using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class UserSocialNetwork
    {
        [Key, Column(Order=1)]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [Key, Column(Order = 2)]
        public Guid SocialNetworkId { get; set; }
        [ForeignKey("SocialNetworkId")]
        public SocialNetwork SocialNetwork { get; set; }
        [MaxLength(100)]
        public string UserNetworkId { get; set; }
        public string UserAccessToken { get; set; }
        public bool EnableSharing { get; set; }
    }
}
