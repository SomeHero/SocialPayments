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
            public List<ProfileSectionResponse> ProfileSections { get; set; }
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
        public class ProfileSectionResponse
        {
            public int Id { get; set; }
            public string SectionHeader { get; set; }
            public int SortOrder { get; set; }

            public virtual List<ProfileItemResponse> ProfileItems { get; set; }
        }
        public class ProfileItemResponse
        {
            public int Id { get; set; }
            public string Label { get; set; }
            public Guid UserAttributeId { get; set; }
            public int SortOrder { get; set; }
        }
    }
}