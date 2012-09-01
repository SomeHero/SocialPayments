using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using System.Net;
using NLog;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class ApplicationsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/applications
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/applications/5
        public HttpResponseMessage<ApplicationModels.ApplicationResponse> Get(String id)
        {
            var applicationServices = new DomainServices.ApplicationService();
            var profileServices = new DomainServices.ProfileServices();

            Domain.Application application = null;
            List<Domain.ProfileSection> profileSections = null;
            HttpResponseMessage<ApplicationModels.ApplicationResponse> response = null;

            try
            {
                application = applicationServices.GetApplication(id);

                if (application == null)
                    throw new NotFoundException(String.Format("Application {0} Not Found", id));

                profileSections = profileServices.GetProfileSections();
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Application {0}.  Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            
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
                    ConfigurationType = c.ConfigurationType,
                    Id = c.Id
                }).ToList(),
                ProfileSections = profileSections.Select(a => new ApplicationModels.ProfileSectionResponse() {
                    Id = a.Id,
                    SectionHeader = a.SectionHeader,
                    SortOrder = a.SortOrder,
                    ProfileItems = a.ProfileItems.Select(i  => new ApplicationModels.ProfileItemResponse() {
                        Id = i.Id,
                        Label = i.Label,
                        SortOrder = i.SortOrder,
                        UserAttributeId =  i.UserAttribute.Id,
                        Points = i.UserAttribute.Points,
                        ItemType = i.ProfileItemType.ToString(),
                        SelectOptionHeader = (i.SelectOptionHeader != null ? i.SelectOptionHeader : ""),
                        SelectOptionDescription = (i.SelectOptionDescription != null ? i.SelectOptionDescription : ""),
                        SelectOptions = i.SelectOptions
                            .OrderBy(s => s.SortOrder)
                            .Select(s => s.OptionValue).ToList()
                    })
                    .OrderBy(i => i.SortOrder)
                    .ToList()
                }).ToList()
            }, HttpStatusCode.OK);
        }

        // POST /api/applications
        public HttpResponseMessage Post(ApplicationModels.SubmitApplicationRequest request)
        {
            DomainServices.ApplicationService applicationServices = new DomainServices.ApplicationService();
            HttpResponseMessage response = null;

            try
            {
                applicationServices.AddApplication(request.name, request.url, request.isActive);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Application {0}.  Exception {1}.", request.name, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Application {0}.  Exception {1}.", request.name, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Application {0}.  Exception {1}. Stack Trace {2}", request.name, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.Created);

            return response;
        }

        // PUT /api/applications/5
        public HttpResponseMessage Put(String id, ApplicationModels.UpdateApplicationRequest request)
        {
            DomainServices.ApplicationService applicationServices = new DomainServices.ApplicationService();
            Domain.Application application = null;
            HttpResponseMessage response = null;

            try
            {
                application = applicationServices.GetApplication(id);

                application.ApplicationName = request.name;
                application.Url = request.url;
                application.IsActive = request.isActive;

                applicationServices.UpdateApplication(application);

            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Updating Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Updating Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Updating Application {0}.  Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        // DELETE /api/applications/5
        public HttpResponseMessage Delete(string id)
        {
            DomainServices.ApplicationService applicationServices = new DomainServices.ApplicationService();
            Domain.Application application = null;
            HttpResponseMessage response = null;

            try
            {
                application = applicationServices.GetApplication(id);

                if(application == null)
                    throw new NotFoundException(String.Format("Invalid Application {0}", id));

                applicationServices.DeleteApplication(id);

            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Application {0}.  Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }
    }
}
