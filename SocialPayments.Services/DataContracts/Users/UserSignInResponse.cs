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
        [DataMember(Name = "paymentAccountId")]
        public string PaymentAccountId { get; set; }
    }
}