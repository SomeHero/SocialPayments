using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.ResponseModels
{
    public class AccountModels
    {
        public class AccountResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
            public string Status { get; set; }
            public string BankIconUrl { get; set; }
        }
    }
}