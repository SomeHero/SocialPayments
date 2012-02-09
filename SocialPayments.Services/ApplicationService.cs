using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using SocialPayments.Services.ServiceContracts;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]

    public class ApplicationService : IApplicationService
    {
        DomainServices.ApplicationService applicationDomainService = new DomainServices.ApplicationService();

        public DataContracts.Application.ApplicationResponse AddApplication(DataContracts.Application.ApplicationRequest applicationRequest)
        {
            var application = applicationDomainService.AddApplication(applicationRequest.ApplicationName, applicationRequest.Url,
                applicationRequest.IsActive);

            return new DataContracts.Application.ApplicationResponse()
            {
                ApplicationName = application.ApplicationName,
                ApiKey = application.ApiKey.ToString(),
                IsActive = application.IsActive,
                Url = application.Url
            };
        }

        public List<DataContracts.Application.ApplicationResponse> GetApplications()
        {
            var Applications = applicationDomainService.GetApplications();
            return Applications.Select(x => new DataContracts.Application.ApplicationResponse { ApiKey = x.ApiKey.ToString(), ApplicationName = x.ApplicationName, Url = x.Url, IsActive = x.IsActive }).ToList();
        }

        public DataContracts.Application.ApplicationResponse GetApplication(string apiKey)
        {
            var application = applicationDomainService.GetApplication(new Guid(apiKey));

            return new DataContracts.Application.ApplicationResponse()
            {
                ApiKey = application.ApiKey.ToString(),
                ApplicationName = application.ApplicationName,
                IsActive = application.IsActive,
                Url = application.Url
            };
        }

        public void UpdateApplication(DataContracts.Application.ApplicationRequest applicationRequest)
        {
            applicationDomainService.UpdateApplication(applicationRequest.ApplicationName, new Guid(applicationRequest.ApiKey), applicationRequest.Url, applicationRequest.IsActive);    
        }

        public void DeleteApplication(DataContracts.Application.ApplicationRequest applicationRequest)
        {
            applicationDomainService.DeleteApplication(new Guid(applicationRequest.ApiKey));
        }
    }
}