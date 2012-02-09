using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserChangeSecurityPinRequest
    {
        [DataMember(Name="userId")]
        public string UserId { get; set; }

        [DataMember(Name="securityPin")]
        public string SecurityPin { get; set; }
    }
}