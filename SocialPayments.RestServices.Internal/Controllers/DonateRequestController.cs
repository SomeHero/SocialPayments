using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using SocialPayments.DomainServices.CustomExceptions;
using SocialPayments.RestServices.Internal.Models;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class DonateRequestController : ApiController
    {
         private Logger _logger = LogManager.GetCurrentClassLogger();


        // GET /api/donaterequest/5
        [HttpGet]
        public HttpResponseMessage Get(String id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }

        // POST /api/donaterequest
        [HttpPost]
        public HttpResponseMessage Post(DonateRequestModels.AddDonateRequest request)
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

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Application {0}.  Exception {1}.", request.name, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Application {0}.  Exception {1}. Stack Trace {2}", request.name, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            
            }

            response = Request.CreateResponse(HttpStatusCode.Created);

            return response;
        }

        // PUT /api/donaterequest/5
        [HttpPut]
        public HttpResponseMessage Put(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/donaterequest/5
        [HttpDelete]
        public HttpResponseMessage Delete(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }
    }
}
