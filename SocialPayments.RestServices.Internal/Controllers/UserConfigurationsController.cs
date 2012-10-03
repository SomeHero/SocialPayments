using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using NLog;
using System.Collections.ObjectModel;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserConfigurationsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/users/{userId}/configurations
        [HttpGet]
        public HttpResponseMessage Get(string userId)
        {
            var userConfigurationServices = new DomainServices.UserConfigurationServices();
            Collection<Domain.UserConfiguration> configItems = null;

            try
            {
                configItems = userConfigurationServices.GetUserConfigurationItems(userId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting User Configuration Setting for User {0}. Exception {1}", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting User Configuration Setting for User {0}. Exception {1}", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting User Configuration Setting for User {0}. Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<List<UserModels.UserConfigurationResponse>>(HttpStatusCode.OK, configItems.Select(u => new UserModels.UserConfigurationResponse()
                {
                    Id = u.Id.ToString(),
                    UserId = u.UserId.ToString(),
                    ConfigurationKey = u.ConfigurationKey,
                    ConfigurationValue = u.ConfigurationValue,
                    ConfigurationType = u.ConfigurationType
                }).ToList());
        }

        // GET /api/users/{userId}/configurations/{id}
        [HttpGet]
        public HttpResponseMessage Get(string userId, string id)
        {
            var userConfigurationServices = new DomainServices.UserConfigurationServices();
            Domain.UserConfiguration configItem = null;

            try
            {
                configItem = userConfigurationServices.GetUserConfigurationItem(userId, id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting User Configuration Setting {0} for User {1}. Exception {2}", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting User Configuration Setting {0} for User {1}. Exception {2}", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting User Configuration Setting for User {0} for User {1}. Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<UserModels.UserConfigurationResponse>(HttpStatusCode.OK,
                new UserModels.UserConfigurationResponse()
            {
                Id = configItem.Id.ToString(),
                UserId = configItem.UserId.ToString(),
                ConfigurationKey = configItem.ConfigurationKey,
                ConfigurationValue = configItem.ConfigurationValue,
                ConfigurationType = configItem.ConfigurationType
            });

        }

        // POST /api/users/{userId}/configurations
        [HttpPost]
        public HttpResponseMessage Post(string userId)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // PUT /api/users/{userId}/configurations/
        [HttpPut]
        public HttpResponseMessage Put(string userId, UserModels.UpdateUserConfigurationRequest request)
        {
            var userConfigurationServices = new DomainServices.UserConfigurationServices();

            try
            {
                userConfigurationServices.UpdateConfigurationItem(userId, request.Key, request.Value);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Updating User Configuration Setting for User {0}. Exception {1}", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Updating User Configuration Setting for User {0}. Exception {1}", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Updating User Configuration Setting for User {0}. Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // DELETE /api/users/{userId}/configurations/{id}
        [HttpDelete]
        public HttpResponseMessage Delete(string userId, string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented); ;
        }
    }
}
