using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.ServiceContracts
{
    [DataContract]
    public class UserRegistrationResponse
    {
        [DataMember(Name="userId")]
        public string UserId { get; set; }

        [DataMember(Name="success")]
        public bool Success { get; set; }

        [DataMember(Name="message")]
        public string Message { get; set; }
    }
}
