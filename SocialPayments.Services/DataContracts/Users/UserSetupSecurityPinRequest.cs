using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserSetupSecurityPinRequest
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "securityPin")]
        public string SecurityPin { get; set; }
    }
}