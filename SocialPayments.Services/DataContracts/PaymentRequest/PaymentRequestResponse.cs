using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.PaymentRequest
{
    [DataContract]
    public class PaymentRequestResponse
    {
        [DataMember(Name = "success")]
        public Boolean Success { get; set; }

        [DataMember(Name = "message")]
        public String Message { get; set; }
    }
}