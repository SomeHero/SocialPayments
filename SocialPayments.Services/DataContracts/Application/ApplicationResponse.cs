using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Application
{
    [DataContract]
    public class ApplicationResponse
    {
        [DataMember(Name="applicationName")]
        public string ApplicationName { get; set; }
        [DataMember(Name="Url")]
        public string Url { get; set; }
        [DataMember(Name="ApiKey")]
        public string ApiKey { get; set; }
        [DataMember(Name="isActive")]
        public bool IsActive { get; set; }
    }
}