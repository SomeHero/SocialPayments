using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.RestServices.External.Models;
using SocialPayments.DataLayer;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.External.Controllers
{
    public class ApplicationsController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/applications
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // GET /api/applications/5
        public HttpResponseMessage<ApplicationModels.ApplicationResponse> Get(String id)
        {
            var application = GetApplication(id);

            if (application == null)
            {
                var response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = "Invalid Application";

                return response;
            }

            //TODO: check to make sure passed in id is the calling application

            return new HttpResponseMessage<ApplicationModels.ApplicationResponse>(new ApplicationModels.ApplicationResponse()
            {
                apiKey = application.ApiKey.ToString(),
                id = application.ApiKey.ToString(),
                isActive = application.IsActive,
                url = application.Url
            }, HttpStatusCode.OK);
        }

        // POST /api/applications
        public HttpResponseMessage Post(string value)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // PUT /api/applications/5
        public HttpResponseMessage Put(String id, ApplicationModels.UpdateApplicationRequest request)
        {
            var application = GetApplication(id);

            if (application == null)
            {
                var response = new HttpResponseMessage<ApplicationModels.ApplicationResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = "Invalid Application";

                return response;
            }

            //TODO: check to make sure passed in id is the calling application

            try
            {
                application.Url = request.url;
            }
            catch (Exception ex)
            {
                //TODO: log exception

                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = ex.Message;

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/applications/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }
        private Application GetApplication(String id)
        {
            Guid applicationId;

            Guid.TryParse(id, out applicationId);

            if (applicationId == null)
                return null;

            Application application = null;

            try
            {
                application = _ctx.Applications.FirstOrDefault(a => a.ApiKey.Equals(applicationId));
            } 
            catch(Exception ex) {
                //TODO: log exception
            }

            if (application == null)
                return null;

            return application;
        }
    }
}
