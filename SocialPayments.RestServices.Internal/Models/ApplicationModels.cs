using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class ApplicationModels
    {
        public class ApplicationResponse {
            public String id { get; set; }
            public String url { get; set; }
            public String apiKey { get; set; }
            public bool isActive { get; set; }
            public List<ApplicationConfigurationResponse> ConfigurationVariables { get; set; }
            public List<UserModels.UserAttribute> ProfileItems { get; set; }
        }
        public class SubmitApplicationRequest
        {
            public string name { get; set; }
            public String url { get; set; }
            public bool isActive { get; set; }
        }
        public class UpdateApplicationRequest
        {
            public string name { get; set; }
            public string url { get; set; }
            public bool isActive { get; set; }
        }
        public class ApplicationConfigurationResponse
        {
            public string Id { get; set; }
            public string ApiKey { get; set; }
            public string ConfigurationKey { get; set; }
            public string ConfigurationValue { get; set; }
            public string ConfigurationType { get; set; }
        }
        public class UpdateApplicationConfigurationRequest
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}