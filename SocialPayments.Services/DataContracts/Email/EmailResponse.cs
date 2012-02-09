using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Email
{
    [ServiceContract]
    public class EmailResponse
    {
        [DataMember(Name = "emailLogId")]
        public Guid EmailLogId { get; set; }

        [DataMember(Name = "application")]
        public DataContracts.Application.ApplicationResponse Application { get; set; }

        [DataMember(Name = "fromAddress")]
        public string FromAddress { get; set; }

        [DataMember(Name = "toAddress")]
        public string ToAddress { get; set; }

        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "sentDate")]
        public DateTime? SentDate { get; set; }

        [DataMember(Name = "lastUpdatedDate")]
        public DateTime? LastUpdatedDate { get; set; }

    }
}