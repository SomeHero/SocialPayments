using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class ApplicationConfigurationsController : ApiController
    {
        // GET /api/applicationattribute
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/applicationattribute/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/applicationattribute
        public void Post(string value)
        {
        }

        // PUT /api/applicationattribute/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/applicationattribute/5
        public void Delete(int id)
        {
        }
    }
}
