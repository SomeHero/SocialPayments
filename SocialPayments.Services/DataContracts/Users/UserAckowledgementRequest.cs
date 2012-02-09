using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.ServiceContracts
{
    [DataContract]
    public class UserAckowledgementRequest
    {
        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }
    }
}
