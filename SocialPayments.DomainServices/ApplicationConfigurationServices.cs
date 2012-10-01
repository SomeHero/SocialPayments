using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Collections.ObjectModel;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.DomainServices
{
    public class ApplicationConfigurationServices
    {
        public Collection<Domain.ApplicationConfiguration> GetApplicationConfigurationSettings(string apiKey)
        {
            using (var ctx = new Context())
            {
                var applicationService = new DomainServices.ApplicationService();
                var application = applicationService.GetApplication(apiKey);

                if (application == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Application {0} Not Found", apiKey));

                return application.ConfigurationValues;
            }
        }
        public Domain.ApplicationConfiguration GetApplicationConfigurationSetting(string apiKey, string id)
        {
            using (var ctx = new Context())
            {
                var applicationService = new DomainServices.ApplicationService();
                var application = applicationService.GetApplication(apiKey);

                var configItem = application.ConfigurationValues
                    .FirstOrDefault(u => u.ConfigurationKey == id);

                if (configItem == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Application Configuration {0} Not Found", id));


                return configItem;
            }
        }
        public void UpdateConfigurationSetting(string apiKey, string configKey, string configValue)
        {
            using (var ctx = new Context())
            {
                var applicationService = new DomainServices.ApplicationService();

                var application = applicationService.GetApplication(apiKey);

                if (application == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Application {0} Not Found", apiKey));

                var itemToUpdate = application.ConfigurationValues
                    .FirstOrDefault(u => u.ConfigurationKey == configKey);

                if (itemToUpdate == null)
                {
                    itemToUpdate = new Domain.ApplicationConfiguration()
                    {
                        Id = Guid.NewGuid(),
                        ApiKey = application.ApiKey,
                        ConfigurationKey = configKey,
                        ConfigurationValue = configValue
                    };
                    application.ConfigurationValues.Add(itemToUpdate);
                }
                else
                {
                    itemToUpdate.ConfigurationValue = configValue;
                }

                ctx.SaveChanges();
            }
        }
    }
}
