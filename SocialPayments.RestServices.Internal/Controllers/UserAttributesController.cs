using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NLog;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserAttributesController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();


        // POST /api/Users/{0}/attributes
        [HttpPost]
        public void Post(string userId, List<UserModels.UserAttribute> userAttributes)
        {
            _logger.Log(LogLevel.Info, String.Format("Number of user Attributes {0}", userAttributes.Count()));
        }

        // PUT /api/Users/{0}/userattributes/{1}
        [HttpPut]
        public HttpResponseMessage Put(string userId, string id, UserModels.UpdateUserAttributeRequest request)
        {
            var userAttributeServices = new DomainServices.UserAttributesServices();
            
            try
            {
                userAttributeServices.UpdateUserAttribute(userId, id, request.AttributeValue);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Application {0}.  Exception {1}.", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Application {0}.  Exception {1}.", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Application {0}.  Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new UserModels.UpdateUserAttributeResponse()
            {
                AttributeKey = id,
                AttributeValue = request.AttributeValue
            });

        }
    }
}
