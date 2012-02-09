using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserRequest
    {
        [DataMember(Name = "userId")]
        public Guid UserId { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "securityPin")]
        public string SecurityPin { get; set; }

        [DataMember(Name = "isLockedOut")]
        public bool IsLockedOut { get; set; }

        [DataMember(Name = "userStatus")]
        public string UserStatus { get; set; }

        [DataMember(Name = "routingNumber")]
        public string RoutingNumber { get; set; }

        [DataMember(Name = "accountNumber")]
        public string AccountNumber { get; set; }

        [DataMember(Name = "nameOnAccount")]
        public string NameOnAccount { get; set; }
    }
}