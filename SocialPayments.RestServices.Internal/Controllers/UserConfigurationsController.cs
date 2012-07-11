using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using SocialPayments.DataLayer;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserConfigurationsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/users/{userId}/configurations
        public HttpResponseMessage<List<UserModels.UserConfigurationResponse>> Get(string userId)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                {
                    var responseMessage = new HttpResponseMessage<List<UserModels.UserConfigurationResponse>>(HttpStatusCode.NotFound);
                    responseMessage.ReasonPhrase = String.Format("User {0} Not Found", userId);

                    return responseMessage;
                }

                return new HttpResponseMessage<List<UserModels.UserConfigurationResponse>>(user.UserConfigurations.Select(u => new UserModels.UserConfigurationResponse()
                {
                    Id = u.Id.ToString(),
                    UserId = u.UserId.ToString(),
                    ConfigurationKey = u.ConfigurationKey,
                    ConfigurationValue = u.ConfigurationValue,
                    ConfigurationType = u.ConfigurationType
                }).ToList(), HttpStatusCode.OK);
            }
        }

        // GET /api/users/{userId}/configurations/{id}
        public HttpResponseMessage<UserModels.UserConfigurationResponse> Get(string userId, string id)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if(user == null)
                {
                    var responseMessage = new HttpResponseMessage<UserModels.UserConfigurationResponse>(HttpStatusCode.NotFound);
                    responseMessage.ReasonPhrase = String.Format("User {0} Not Found", userId);

                    return responseMessage;
                }

                var configItem = user.UserConfigurations
                    .FirstOrDefault(u => u.ConfigurationKey == id);

                if(configItem == null)
                {
                    var responseMessage = new HttpResponseMessage<UserModels.UserConfigurationResponse>(HttpStatusCode.NotFound);
                    responseMessage.ReasonPhrase = String.Format("Configuration Item {0} Not Found", id);

                    return responseMessage;
                }

                return new HttpResponseMessage<UserModels.UserConfigurationResponse>(new UserModels.UserConfigurationResponse()
                {
                    Id = configItem.Id.ToString(),
                    UserId = configItem.UserId.ToString(),
                    ConfigurationKey = configItem.ConfigurationKey,
                    ConfigurationValue = configItem.ConfigurationValue,
                    ConfigurationType =configItem.ConfigurationType
                }, HttpStatusCode.OK);
            }
        }

        // POST /api/users/{userId}/configurations
        public HttpResponseMessage Post(string userId)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // PUT /api/users/{userId}/configurations/
        public HttpResponseMessage Put(string userId, UserModels.UpdateUserConfigurationRequest request)
        {
            using(var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);
 
                    var itemToUpdate = user.UserConfigurations
                        .FirstOrDefault(u => u.ConfigurationKey == request.Key);

                    if (itemToUpdate == null)
                    {
                        itemToUpdate = new Domain.UserConfiguration()
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.UserId,
                            ConfigurationKey = request.Key,
                            ConfigurationValue = request.Value
                        };
                        user.UserConfigurations.Add(itemToUpdate);
                    }
                    else
                    {
                        itemToUpdate.ConfigurationValue = request.Value;
                    }

                ctx.SaveChanges();
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/users/{userId}/configurations/{id}
        public HttpResponseMessage Delete(string userId, string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented); ;
        }
    }
}
