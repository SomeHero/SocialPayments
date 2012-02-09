using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserForgotPasswordResponse
    {
        [DataMember(Name = "error")]
        public bool Errors { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}