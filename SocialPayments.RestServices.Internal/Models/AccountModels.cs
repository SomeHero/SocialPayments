using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class AccountModels
    {
        public class AccountResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string BankName { get; set; }
            public string BankIconUrl { get; set; }
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
            public string Status { get; set; }
        }
        public class AddAccountRequest
        {
            public string ApiKey { get; set; }
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
            public string SecurityPin { get; set; }
        }
        public class SubmitAccountRequest
        {
            public string ApiKey { get; set; }
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
            public string SecurityPin { get; set; }
            public int SecurityQuestionID { get; set; }
            public string SecurityQuestionAnswer { get; set; }
        }
        public class SubmitAccountResponse
        {
            public string paymentAccountId { get; set; }
        }
        public class UpdateAccountRequest
        {
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountType { get; set; }
            public string Status { get; set; }
        }
        public class ChangePreferredAccountRequest
        {
            public string PaymentAccountId { get; set; }
            public string SecurityPin { get; set; }
        }
    }
}