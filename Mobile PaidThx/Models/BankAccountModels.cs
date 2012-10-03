using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;
using System.Web.Mvc;

namespace Mobile_PaidThx.Models
{
    public class BankAccountModels
    {
        public class BankAccountsModel
        {
            public List<BankAccountModel> PaymentAccounts { get; set; }
            public string PreferredSendAccountId { get; set; }
            public string PreferredReceiveAccountId { get; set; }
        }
        public class AddPaymentAccountModel
        {
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string ConfirmAccountNumber { get; set; }
            public string AccountType { get; set; }
            public string DefaultRecieve { get; set; }
            public string DefaultSend { get; set; }
        }
        public class EditPaymentAccountModel
        {
            public string PaymentAccountId { get; set; }
            public string Nickname { get; set; }
            public string NameOnAccount { get; set; }
            public string RoutingNumber { get; set; }
            public string AccountNumber { get; set; }
            public string AccountType { get; set; }
            public string DefaultRecieve { get; set; }
            public string DefaultSend { get; set; }
            public SelectList AccountTypeOptions { get; set; }
            public List<KeyValuePair<string, string>> Options { get; set; }
        }
        public class DeletePaymentAccountModel
        {
            public string PaymentAccountId { get; set; }
        }
        public class PinSwipeModel
        {
            public string PinCode { get; set; }
        }
        public class SetPreferredSendAccountModel
        {
            public string PaymentAccountId { get; set; }
        }
        public class SetPreferredReceiveAccountModel
        {
            public string PaymentAccountId { get; set; }
        }
        public class VerifyAccountModel
        {
            public string Amount1 { get; set; }
            public string Amount2 { get; set; }
        }
    }
}