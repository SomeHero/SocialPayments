using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Net;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class ApplicationsController : ApiController
    {
        private Context _ctx = new Context();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/applications
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/applications/5
        public HttpResponseMessage<ApplicationModels.ApplicationResponse> Get(String id)
        {

            DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService(_ctx);

            Guid apiKey;

            Guid.TryParse(id, out apiKey);

            if (apiKey == null)
            {
                _logger.Log(LogLevel.Error, String.Format("Unable to parse Guid {0}", id));

                return new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
            }

            Application application;

            try
            {
                application = applicationService.GetApplication(apiKey.ToString());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled exception getting application {0}. {1}", id, ex.Message));

                var response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            if (application == null)
            {
                var response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = "Invalid Application";

                return response;
            }

            var profileSections = _ctx.ProfileSections
                .Select(u => u)
                .OrderBy(u => u.SortOrder)
                .ToList();

            //TODO: check to make sure passed in id is the calling application

            return new HttpResponseMessage<ApplicationModels.ApplicationResponse>(new ApplicationModels.ApplicationResponse()
            {
                apiKey = application.ApiKey.ToString(),
                id = application.ApiKey.ToString(),
                isActive = application.IsActive,
                url = application.Url,
                ConfigurationVariables = application.ConfigurationValues.Select(c => new ApplicationModels.ApplicationConfigurationResponse() {
                    ApiKey = c.ApiKey.ToString(),
                    ConfigurationKey = c.ConfigurationKey,
                    ConfigurationValue = c.ConfigurationValue,
                    ConfigurationType = c.ConfigurationType
                }).ToList(),
                ProfileSections = profileSections.Select(a => new ApplicationModels.ProfileSectionResponse() {
                    Id = a.Id,
                    SectionHeader = a.SectionHeader,
                    SortOrder = a.SortOrder,
                    ProfileItems = a.ProfileItems.Select(i  => new ApplicationModels.ProfileItemResponse() {
                        Id = i.Id,
                        Label = i.Label,
                        SortOrder = i.SortOrder,
                        UserAttributeId =  i.UserAttribute.Id
                    })
                    .OrderBy(i => i.SortOrder)
                    .ToList()
                }).ToList()
            }, HttpStatusCode.OK);
        }

        // POST /api/applications
        public HttpResponseMessage Post(ApplicationModels.SubmitApplicationRequest request)
        {
            DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService(_ctx);
            Domain.Application application;

            try
            {
                application = applicationService.AddApplication(request.name, request.url, request.isActive);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled exception adding application. {0}", ex.Message));

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        // PUT /api/applications/5
        public HttpResponseMessage Put(String id, ApplicationModels.UpdateApplicationRequest request)
        {
            DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService(_ctx);

            Guid apiKey;

            Guid.TryParse(id, out apiKey);

            if(apiKey == null)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Updating Application.  ApiKey {0} could not be parsed as a GUID.", id));

                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = "Invalid ApiKey.  Cannot be parsed to GUID.";

                return message;
            }

            Application application = null;

            try
            {
                application = applicationService.GetApplication(id);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Application.  Application with the API Key {0} not found. {0}", id, ex.Message));

                var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                message.ReasonPhrase = "Application Resource Not Found.";

                return message;
            }


            try
            {
                application.IsActive = request.isActive;
                application.ApplicationName = request.name;
                application.Url = request.url;

                applicationService.UpdateApplication(application);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception updating application {0}. {1}", id, ex.Message));

                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = ex.Message;

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/applications/5
        public HttpResponseMessage Delete(string id)
        {
            DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService(_ctx);

            Guid apiKey;

            Guid.TryParse(id, out apiKey);

            if(apiKey == null)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Application.  ApiKey could not be parsed as a GUID. {0}", id));

                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = "Invalid ApiKey.  Cannot be parsed to GUID.";

                return message;
            }

            try
            {
                applicationService.DeleteApplication(apiKey);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Application. {0}", ex.Message));

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
