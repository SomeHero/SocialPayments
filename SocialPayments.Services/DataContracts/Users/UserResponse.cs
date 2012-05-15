using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Users
{
    [DataContract]
    public class UserResponse
    {
        [DataMember(Name = "userId")]
        public Guid UserId { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "isLockedOut")]
        public bool IsLockedOut { get; set; }

        [DataMember(Name = "userStatus")]
        public string UserStatus { get; set; }

        [DataMember(Name = "routingNumber")]
        public string RoutingNumber { get; set; }

        [DataMember(Name = "accountNumber")]
        public string AccountNumber { get; set; }

        [DataMember(Name = "firstName")]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName")]
        public string LastName { get; set; }

        [DataMember(Name = "totalMoneyReceived")]
        public double TotalMoneyReceived { get; set; }

        [DataMember(Name = "totalMoneySent")]
        public double TotalMoneySent { get; set; }
    }
}