using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using NLog;
using SocialPayments.RestServices.Internal.Models;
using System.Collections.ObjectModel;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class ApplicationConfigurationsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/applications/{apiKey}/configurations
        public HttpResponseMessage<List<ApplicationModels.ApplicationConfigurationResponse>> Get(string apiKey)
        {
            var applicationConfigurationServices = new DomainServices.ApplicationConfigurationServices();

            Collection<Domain.ApplicationConfiguration> configItems = null;
            HttpResponseMessage response= null; 

            try
            {
                configItems = applicationConfigurationServices.GetApplicationConfigurationSettings(apiKey);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Application Configuration for Application {0}. Exception {1}. Stack Trace {2}", apiKey, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Application Configuration for Application {0}. Exception {1}. Stack Trace {2}", apiKey, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Application Configuration for Application {0}. Exception {1}. Stack Trace {2}", apiKey, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<ApplicationModels.ApplicationConfigurationResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
            }

            return new HttpResponseMessage<List<ApplicationModels.ApplicationConfigurationResponse>>(configItems.Select(u => new ApplicationModels.ApplicationConfigurationResponse()
            {
                Id = u.Id,
                ApiKey = u.ApiKey.ToString(),
                ConfigurationKey = u.ConfigurationKey,
                ConfigurationValue = u.ConfigurationValue,
                ConfigurationType = u.ConfigurationType
            }).ToList(), HttpStatusCode.OK);
        }

        // GET /api/applications/{apiKey}/{id}
        public HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse> Get(string apiKey, string id)
        {
            var applicationConfigurationServices = new DomainServices.ApplicationConfigurationServices();

            Domain.ApplicationConfiguration configItem = null;
            HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse> response = null;

            try
            {
                configItem = applicationConfigurationServices.GetApplicationConfigurationSetting(apiKey, id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Application Configuration {0} for Application {1}. Exception {2}.", id, apiKey, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Application Configuration {0} for Application {1}. Exception {2}.", id, apiKey, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Application Configuration for {0} for Application {1}. Exception {2}. Stack Trace {3}", id, apiKey, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
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

        // POST /api/applications/{apiKey}/configurations
        public HttpResponseMessage Post(string apiKey)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // PUT /api/applications/{apiKey}/configurations/
        public HttpResponseMessage Put(string apiKey, ApplicationModels.UpdateApplicationConfigurationRequest request)
        {
            var applicationConfigurationServices= new DomainServices.ApplicationConfigurationServices();
            HttpResponseMessage response = null;

            try
            {
                applicationConfigurationServices.UpdateConfigurationSetting(apiKey, request.Key, request.Value);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Updating Application Configuration Setting for Application {0} with Key {1} and Value {2}. Exception {3}. Stack Trace {4}",
                    apiKey, request.Key, request.Value, ex.Message, ex.StackTrace));
                
                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Updating Application Configuration Setting for Application {0} with Key {1} and Value {2}. Exception {3}. Stack Trace {4}",
                    apiKey, request.Key, request.Value, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Updating Application Configuration Setting for Application {0} with Key {1} and Value {2}. Exception {3}. Stack Trace {4}",
                    apiKey, request.Key, request.Value, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationConfigurationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
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
