using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NLog;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using System.Net;

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
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                {
                    var message = String.Format("User Id {0} is not valid", userId);

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

                    return responseMessage;
                }

                Guid userAttributeId;

                Guid.TryParse(id, out userAttributeId);

                if(userAttributeId == null)
                {
                    var message = String.Format("Attribute Id {0} is not valid", id);

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

                    return responseMessage;
                }
                var userAttribute = user.UserAttributes.FirstOrDefault(a => a.UserAttributeId.Equals(userAttributeId));

                if(userAttribute == null)
                {
                    user.UserAttributes.Add(new Domain.UserAttributeValue()
                    {
                        id = Guid.NewGuid(),
                        AttributeValue = request.AttributeValue,
                        UserAttributeId = userAttributeId
                    });

                    ctx.SaveChanges();

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                userAttribute.AttributeValue = request.AttributeValue;

                ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        // DELETE /api/userattribute/5
        public void Delete(int id)
        {
        }
    }
}
