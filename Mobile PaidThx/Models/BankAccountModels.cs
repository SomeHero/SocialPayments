﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

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
            public SelectListItem[] AccountTypeOptions { get; set; }
        }
        public class PinSwipeModel
        {
            public string PinCode { get; set; }
        }
    }
}