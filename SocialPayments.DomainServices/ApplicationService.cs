using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace SocialPayments.DomainServices
{
    public class ApplicationService
    {

        public ApplicationService() { }

        public Application AddApplication(string applicationName, string url, bool isActive)
        {
            using (var ctx = new Context())
            {
                var application = ctx.Applications.Add(new Application()
                {
                    ApiKey = Guid.NewGuid(),
                    ApplicationName = applicationName,
                    IsActive = isActive,
                    Url = url
                });

                ctx.SaveChanges();

                return application;
            }

        }
        public List<Application> GetApplications()
        {
            using (var ctx = new Context())
            {
                return ctx.Applications.Select(a => a).ToList<Application>();
            }
        }
        public Application GetApplication(string apiKey)
        {
            using (var ctx = new Context())
            {
                Guid apiKeyGuid;

                Guid.TryParse(apiKey, out apiKeyGuid);

                if (apiKeyGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Invalid Application Specified {0}.", apiKey));

                return ctx.Applications
                    .Include("ConfigurationValues")
                    .FirstOrDefault(a => a.ApiKey.Equals(apiKeyGuid));
            }
        }
        public void AddApplication(string name, string url, string isActive)
        {
            using (var ctx = new Context())
            {
                DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();
                
                applicationService.AddApplication(name, url, isActive);
            }
        }
        public void UpdateApplication(Application application)
        {
            using (var ctx = new Context())
            {
                application.LastUpdatedDate = System.DateTime.Now;

                ctx.SaveChanges();
            }
        }
        public void DeleteApplication(String apiKey)
        {
            using (var ctx = new Context())
            {

                var application = GetApplication(apiKey);

                if (application == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Invalid Application Specified {0}.", apiKey));

                ctx.Applications.Remove(application);

                ctx.SaveChanges();
            }
        }
    }
}
