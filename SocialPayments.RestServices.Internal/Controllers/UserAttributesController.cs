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

        // GET /api/userattribute
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/userattribute/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/Users/{0}/attributes
        public void Post(string userId, List<UserModels.UserAttribute> userAttributes)
        {
            _logger.Log(LogLevel.Info, String.Format("Number of user Attributes {0}", userAttributes.Count()));
        }

        // PUT /api/Users/{0}/userattributes/{1}
        public HttpResponseMessage Put(string userId, string id, UserModels.UpdateUserAttributeRequest request)
        {
            var userAttributeServices = new DomainServices.UserAttributesServices();
            HttpResponseMessage response = null;
            try
            {
                userAttributeServices.UpdateUserAttribute(userId, id, request.AttributeValue);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Application {0}.  Exception {1}.", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Application {0}.  Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }


            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;

        }

        // DELETE /api/userattribute/5
        public void Delete(int id)
        {
        }
    }
}
