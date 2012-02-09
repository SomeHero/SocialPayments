using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.SMS
{
    [DataContract]
    public class SMSRequest
    {
        [DataMember(Name = "smsMessageId")]
        public Guid SMSMessageId { get; set; }

        [DataMember(Name = "apiKey")]
        public Guid ApiKey { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}