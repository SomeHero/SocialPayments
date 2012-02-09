using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserSetupPasswordResponse
    {
        [DataMember(Name="success")]
        public bool Success { get; set; }

        [DataMember(Name="message")]
        public string Message { get; set; }

    }
}
