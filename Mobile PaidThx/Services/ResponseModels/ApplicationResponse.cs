using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.ResponseModels
{
    public class ApplicationResponse
    {
        public String id { get; set; }
        public String url { get; set; }
        public String apiKey { get; set; }
        public bool isActive { get; set; }
        public List<ApplicationConfigurationResponse> ConfigurationVariables { get; set; }
        public List<ProfileSectionResponse> ProfileSections { get; set; }
    }
    public class ApplicationConfigurationResponse
    {
        public Guid Id { get; set; }
        public string ApiKey { get; set; }
        public string ConfigurationKey { get; set; }
        public string ConfigurationValue { get; set; }
        public string ConfigurationType { get; set; }
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
        public string ItemType { get; set; }
        public int Points { get; set; }
        public string SelectOptionHeader { get; set; }
        public string SelectOptionDescription { get; set; }
        public List<string> SelectOptions { get; set; }
    }
}