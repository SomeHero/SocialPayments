using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.BetaSignUp
{
    [DataContract]
    public class BetaSignUpRequest
    {
        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }
    }
}