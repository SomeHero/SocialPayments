using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using NLog;
using SocialPayments.DataLayer;
using SocialPayments.RestServices.Internal.Models;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class ApplicationConfigurationsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/applications/{apiKey}/configurations
        public HttpResponseMessage<List<ApplicationModels.ApplicationConfigurationResponse>> Get(string apiKey)
        {
            using (var ctx = new Context())
            {
                var applicationService = new DomainServices.ApplicationService(ctx);

                var application = applicationService.GetApplication(apiKey);

                if (application == null)
                {
                    var responseMessage = new HttpResponseMessage<List<ApplicationModels.ApplicationConfigurationResponse>>(HttpStatusCode.NotFound);
                    responseMessage.ReasonPhrase = String.Format("Application {0} Not Found", apiKey);

                    return responseMessage;
                }

                return new HttpResponseMessage<List<ApplicationModels.ApplicationConfigurationResponse>>(application.ConfigurationValues.Select(u => new ApplicationModels.ApplicationConfigurationResponse()
                {
                    Id = u.Id,
                    ApiKey = u.ApiKey.ToString(),
                    ConfigurationKey = u.ConfigurationKey,
                    ConfigurationValue = u.ConfigurationValue,
                    ConfigurationType = u.ConfigurationType
                }).ToList(), HttpStatusCode.OK);
            }
        }

        // GET /api/applications/{apiKey}/{id}
        public HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse> Get(string apiKey, string id)
        {
            using (var ctx = new Context())
            {
                var applicationService = new DomainServices.ApplicationService(ctx);

                var application = applicationService.GetApplication(apiKey);

                if (application == null)
                {
                    var responseMessage = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.NotFound);
                    responseMessage.ReasonPhrase = String.Format("Application {0} Not Found", apiKey);

                    return responseMessage;
                }

                var configItem = application.ConfigurationValues
                    .FirstOrDefault(u => u.ConfigurationKey == id);

                if (configItem == null)
                {
                    var responseMessage = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.NotFound);
                    responseMessage.ReasonPhrase = String.Format("Configuration Item {0} Not Found", id);

                    return responseMessage;
                }

                return new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(new ApplicationModels.ApplicationConfigurationResponse()
                {
                    Id = configItem.Id,
                    ApiKey = configItem.ApiKey.ToString(),
                    ConfigurationKey = configItem.ConfigurationKey,
                    ConfigurationValue = configItem.ConfigurationValue,
                    ConfigurationType = configItem.ConfigurationType
                }, HttpStatusCode.OK);
            }
        }

        // POST /api/applications/{apiKey}/configurations
        public HttpResponseMessage Post(string apiKey)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // PUT /api/applications/{apiKey}/configurations/
        public HttpResponseMessage Put(string apiKey, ApplicationModels.UpdateApplicationConfigurationRequest request)
        {
            using (var ctx = new Context())
            {
                var applicationService = new DomainServices.ApplicationService(ctx);

                var application = applicationService.GetApplication(apiKey);

                var itemToUpdate = application.ConfigurationValues
                    .FirstOrDefault(u => u.ConfigurationKey == request.Key);

                if (itemToUpdate == null)
                {
                    itemToUpdate = new Domain.ApplicationConfiguration()
                    {
                        Id = Guid.NewGuid(),
                        ApiKey = application.ApiKey,
                        ConfigurationKey = request.Key,
                        ConfigurationValue = request.Value
                    };
                    application.ConfigurationValues.Add(itemToUpdate);
                }
                else
                {
                    itemToUpdate.ConfigurationValue = request.Value;
                }

                ctx.SaveChanges();
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/users/{apiKey}/configurations/{id}
        public HttpResponseMessage Delete(string apiKey, string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented); ;
        }
    }
}
