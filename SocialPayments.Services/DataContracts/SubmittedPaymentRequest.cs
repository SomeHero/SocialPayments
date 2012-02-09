using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts
{
    [DataContract]
    public class SubmittedPaymentRequest
    {
        [DataMember(Name = "MessageId")]
        public string MessageId { get; set; }

        [DataMember(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [DataMember(Name = "TopicARN")]
        public string TopicARN { get; set; }

        [DataMember(Name = "Type")]
        public string Type { get; set; }

        [DataMember(Name = "UnsubscribeURL")]
        public string UnsubscribeURL { get; set; }

        [DataMember(Name = "Message")]
        public string Message { get; set; }

        [DataMember(Name = "Subject")]
        public string Subject { get; set; }

        [DataMember(Name = "Signature")]
        public string Signature { get; set; }

        [DataMember(Name = "SignatureVersion")]
        public string SignatureVersion { get; set; }

    }
}