using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Models
{
    public class PaymentAccountModels
   { 
    }
    public class ListPaymentAccountModel
    {
        public ListPaymentAccountModel()
        {
            PaymentAccounts = new List<BankAccountModel>();
        }

        public List<BankAccountModel> PaymentAccounts { get; set; }
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
}