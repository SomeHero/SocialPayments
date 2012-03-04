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

        [DataMember(Name = "accountInformation")]
        public string AccountInformation { get; set; }
    }
}