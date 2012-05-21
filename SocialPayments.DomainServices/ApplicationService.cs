using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;
using System.Collections.ObjectModel;

namespace SocialPayments.DomainServices
{
    public class ApplicationService
    {
        private IDbContext _ctx;

        public ApplicationService() { }

        public ApplicationService(IDbContext context)
        {
            _ctx = context;
        }

        public Application AddApplication(string applicationName, string url, bool isActive)
        {
            var application =_ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = applicationName,
                IsActive = isActive,
                Url = url
            });

            _ctx.SaveChanges();

            return application;
        }
        public List<Application> GetApplications()
        {
            return _ctx.Applications.Select(a => a).ToList<Application>();
        }
        public Application GetApplication(Guid apiKey)
        {
            return _ctx.Applications.FirstOrDefault(a => a.ApiKey.Equals(apiKey));
        }
        public void UpdateApplication(string applicationName, Guid apiKey, string url, bool isActive)
        {
            var application = GetApplication(apiKey);

            application.ApplicationName = applicationName;
            application.Url = url;
            application.IsActive = isActive;

            _ctx.SaveChanges();
        }
        public void DeleteApplication(Guid apiKey)
        {
            var application = GetApplication(apiKey);

            _ctx.Applications.Remove(application);

            _ctx.SaveChanges();
        }
    }
}
