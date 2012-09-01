using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using NLog;
using System.Collections.ObjectModel;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserConfigurationsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/users/{userId}/configurations
        public HttpResponseMessage<List<UserModels.UserConfigurationResponse>> Get(string userId)
        {
            var userConfigurationServices = new DomainServices.UserConfigurationServices();
            Collection<Domain.UserConfiguration> configItems = null;
            HttpResponseMessage<List<UserModels.UserConfigurationResponse>> response = null;
            
            try
            {
                configItems = userConfigurationServices.GetUserConfigurationItems(userId);
            }
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage<List<UserModels.UserConfigurationResponse>>(configItems.Select(u => new UserModels.UserConfigurationResponse()
                {
                    Id = u.Id.ToString(),
                    UserId = u.UserId.ToString(),
                    ConfigurationKey = u.ConfigurationKey,
                    ConfigurationValue = u.ConfigurationValue,
                    ConfigurationType = u.ConfigurationType
                }).ToList(), HttpStatusCode.OK);

            return response;
        }

        // GET /api/users/{userId}/configurations/{id}
        public HttpResponseMessage<UserModels.UserConfigurationResponse> Get(string userId, string id)
        {
            var userConfigurationServices = new DomainServices.UserConfigurationServices();
            Domain.UserConfiguration configItem = null;
            HttpResponseMessage<UserModels.UserConfigurationResponse> response = null;

            try
            {
                configItem = userConfigurationServices.GetUserConfigurationItem(userId, id);
            }
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage<UserModels.UserConfigurationResponse>(new UserModels.UserConfigurationResponse()
            {
                Id = configItem.Id.ToString(),
                UserId = configItem.UserId.ToString(),
                ConfigurationKey = configItem.ConfigurationKey,
                ConfigurationValue = configItem.ConfigurationValue,
                ConfigurationType = configItem.ConfigurationType
            }, HttpStatusCode.OK);

            return response;
        }

        // POST /api/users/{userId}/configurations
        public HttpResponseMessage Post(string userId)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // PUT /api/users/{userId}/configurations/
        public HttpResponseMessage Put(string userId, UserModels.UpdateUserConfigurationRequest request)
        {
            var userConfigurationServices = new DomainServices.UserConfigurationServices();
            HttpResponseMessage response = null;

            try
            {
                userConfigurationServices.UpdateConfigurationItem(userId, request.Key, request.Value);
            }
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        // DELETE /api/users/{userId}/configurations/{id}
        public HttpResponseMessage Delete(string userId, string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented); ;
        }
    }
}
