using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Payment
{
    [DataContract]
    public class CancelPaymentRequest
    {
        [DataMember(Name="paymentId")]
        public string PaymentId { get; set; }
    }
}
