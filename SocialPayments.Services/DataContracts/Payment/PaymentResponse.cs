using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using SocialPayments.Services.DataContracts.Payment.Base;

namespace SocialPayments.Services.DataContracts.Payment
{
    [DataContract]
    public class PaymentResponse
    {
        [DataMember(Name = "responseStatus")]
        public string ResponseStatus { get; set; }

        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "paymentId")]
        public string PaymentId { get; set; }

        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "fromMobileNumber")]
        public string FromMobileNumber { get; set; }

        [DataMember(Name = "toMobileNumber")]
        public string ToMobileNumber { get; set; }

        [DataMember(Name = "amount")]
        public double Amount { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "fromAcount")]
        public DataContracts.PaymentAccount.PaymentAccountReponse FromAccount { get; set; }
    }
}