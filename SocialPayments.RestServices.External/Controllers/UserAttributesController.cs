using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;

namespace SocialPayments.RestServices.External.Controllers
{
    public class UserAttributesController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/users/{id}/attributes
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/users/{id}/attributes/{attributeId}
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/users/{id}/attributes/{attributeId}
        public void Post(string value)
        {
        }

        // PUT /api/users/attributes/{attributeId}
        public void Put(int id, string value)
        {
        }

        // DELETE /api/users/id/attributes/{attributeId}
        public void Delete(int id)
        {
        }
    }
}
