using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserSetupPasswordRequest
    {
        [DataMember(Name="userId")]
        public string UserId { get; set; }
        
        [DataMember(Name="deviceId")]
        public string DeviceId { get; set; }
        
        [DataMember(Name="userName")]
        public string UserName { get; set; }
        
        [DataMember(Name="password")]
        public string Password { get; set; }
    }
}
