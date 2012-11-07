using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.External.Models
{
    public class AccountModels
    {
        public class AccountResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
        }
        public class SubmitAccountRequest
        {
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
        }
        public class UpdateAccountRequest
        {
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
        }
    }
}