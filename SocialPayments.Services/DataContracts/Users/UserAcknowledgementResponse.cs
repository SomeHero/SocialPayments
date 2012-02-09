using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.ServiceContracts
{
    [DataContract]
    public class UserAcknowledgementResponse
    {
        [DataMember(Name = "userId")]
        public String UserId { get; set; }

        [DataMember(Name = "isMobileNumberRegistered")]
        public bool IsMobilieNumberRegistered { get; set; }

        [DataMember(Name = "doesDeviceIdMatch")]
        public bool DoesDeviceIdMatch { get; set; }

        [DataMember(Name = "setupSecurityPin")]
        public bool SetupSecurityPin { get; set; }

        [DataMember(Name = "setupPassword")]
        public Boolean SetupPassword { get; set; }

        [DataMember(Name = "paymentAccountId")]
        public String PaymentAccountId { get; set; }

    }
}
