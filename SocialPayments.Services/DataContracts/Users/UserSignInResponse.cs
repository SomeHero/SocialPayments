using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserSignInResponse
    {
        [DataMember(Name = "isValid")]
        public bool IsValid { get; set; }
        [DataMember(Name = "userId")]
        public string UserId { get; set; }
        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }
        [DataMember(Name = "setupSecurityPin")]
        public bool SetupSecurityPin { get; set; }
        [DataMember(Name = "setupPassword")]
        public Boolean SetupPassword { get; set; }
        [DataMember(Name = "paymentAccountId")]
        public String PaymentAccountId { get; set; }
    }
}