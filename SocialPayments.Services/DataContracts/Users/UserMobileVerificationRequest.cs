using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.ServiceContracts
{
    [DataContract]
    public class UserMobileVerificationRequest
    {
        [DataMember(Name="userId")]
        public string UserId { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "verificationCode1")]
        public string VerificationCode1 { get; set; }

        [DataMember(Name = "verificationCode2")]
        public string VerificationCode2 { get; set; }
    }
}
