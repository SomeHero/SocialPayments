using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.ServiceContracts
{
    [DataContract]
    public class UserRegistrationRequest
    {
        [DataMember(Name = "apiKey")]
        public string ApiKey { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "securityPin")]
        public string SecurityPin { get; set; }

        [DataMember(Name="registrationMethod")]
        public string RegistrationMethod { get; set; }
    }
}
