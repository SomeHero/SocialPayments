using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserForgotPasswordRequest
    {
        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }
    }
}