using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.PaymentAccount
{
    [DataContract]
    public class PaymentAccountReponse
    {
        [DataMember(Name = "paymentAccountId")]
        public string Id { get; set; }

        [DataMember(Name = "nameOnAccount")]
        public string NameOnAccount { get; set; }

        [DataMember(Name = "routingNumber")]
        public string RoutingNumber { get; set; }

        [DataMember(Name = "accountNumber")]
        public string AccountNumber { get; set; }

        [DataMember(Name = "accountType")]
        public string AccountType { get; set; }
    }
}