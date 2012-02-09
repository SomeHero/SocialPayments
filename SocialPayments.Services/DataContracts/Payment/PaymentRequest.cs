using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Payment
{
    [DataContract]
    public class PaymentRequest
    {
        //[DataMember(Name = "ApiKey")]
        //public Guid ApiKey { get; set; }

        [DataMember(Name = "PaymentId")]
        public string PaymentId { get; set; }

        [DataMember(Name="userId")]
        public string UserId { get; set; }

        [DataMember(Name="fromMobileNumber")]
        public string FromMobileNumber { get; set; }

        [DataMember(Name="toMobileNumber")]
        public string ToMobileNumber { get; set; }

        [DataMember(Name="amount")]
        public double Amount { get; set; }

        [DataMember(Name="comment")]
        public string Comment { get; set; }

        [DataMember(Name="fromAcount")]
        public int FromAccount { get; set; }

        [DataMember(Name = "securityPin")]
        public string SecurityPin { get; set; }
    }
}