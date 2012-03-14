using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.BetaSignUp
{
    [DataContract]
    public class BetaSignupResponse
    {
        [DataMember(Name="success")]
        public bool Success { get; set; }

        [DataMember(Name="id")]
        public Guid Id { get; set; }

        [DataMember(Name="emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name="message")]
        public string Message { get; set; }
    }
}