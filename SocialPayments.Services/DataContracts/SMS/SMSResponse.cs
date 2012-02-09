using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.SMS
{
    [DataContract]
    public class SMSResponse
    {
        [DataMember(Name = "smsMessageId")]
        public Guid SMSMessageId { get; set; }

        [DataMember(Name = "application")]
        public DataContracts.Application.ApplicationResponse Application { get; set; }

        [DataMember(Name = "mobileNumber")]
        public string MobileNumber { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "sentDate")]
        public DateTime? SentDate { get; set; }

        [DataMember(Name = "lastUpdatedDate")]
        public DateTime? LastUpdateDate { get; set; }
    }
}