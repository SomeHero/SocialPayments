using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class ApplicationsController : ApiController
    {
        // GET /api/applications
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/applications/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/applications
        public void Post(string value)
        {
        }

        // PUT /api/applications/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/applications/5
        public void Delete(int id)
        {
        }
    }
}
