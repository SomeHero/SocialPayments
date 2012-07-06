using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserConfigurationsController : ApiController
    {
        // GET /api/users/{userId}/configurations
        public IEnumerable<string> Get(string userId)
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/users/{userId}/configurations/{id}
        public string Get(string userId, string id)
        {
            return "value";
        }

        // POST /api/users/{userId}/configurations
        public void Post(string userId)
        {
        }

        // PUT /api/users/{userId}/configurations/{id}
        public void Put(string userId, string id)
        {
        }

        // DELETE /api/users/{userId}/configurations/{id}
        public void Delete(string userId, string id)
        {
        }
    }
}
