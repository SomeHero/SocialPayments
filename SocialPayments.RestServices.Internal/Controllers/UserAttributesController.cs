using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NLog;
using SocialPayments.RestServices.Internal.Models;

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

        // PUT /api/userattribute/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/userattribute/5
        public void Delete(int id)
        {
        }
    }
}
