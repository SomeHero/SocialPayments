using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using SocialPayments.Services.DataContracts.Payment.Base;

namespace SocialPayments.Services.DataContracts.Payment
{
    [DataContract]
    public class FailedPaymentResponse: PaymentResponseBase
    {
        [DataMember(Name = "responseStatus")]
        public string ResponseStatus { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}