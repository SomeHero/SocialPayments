using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.PaymentRequest
{
    [DataContract]
    public class PaymentRequestRequest
    {
        [DataMember(Name = "userId")]
        public String UserId { get; set; }

        [DataMember(Name = "apiKey")]
        public String ApiKey { get; set; }

        [DataMember(Name = "deviceId")]
        public String DeviceId { get; set; }

        [DataMember(Name = "recipientUri")]
        public String RecipientUri { get; set; }

        [DataMember(Name = "amount")]
        public Double Amount { get; set; }

        [DataMember(Name = "comments")]
        public String Comments { get; set; }

        [DataMember(Name = "securityPin")]
        public string SecurityPin { get; set; }
    }
}