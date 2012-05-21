using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using System.Net;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MobileNumberSignUpKeyController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/mobilenumbersignupkey
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/mobilenumbersignupkey/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/mobilenumbersignupkey
        public HttpResponseMessage<MobileNumberSignUpKeyModels.GetKeyResponse> Post(MobileNumberSignUpKeyModels.GetKeyRequest value)
        {
            var signUpKey = _ctx.MobileNumberSignUpKeys.Add(new Domain.MobileNumberSignUpKey() {
                CreateDate = System.DateTime.Now,
                IsExpired = false,
                MobileNumber = "",
                SignUpKey = Guid.NewGuid()
            });

            _ctx.SaveChanges();

            return new HttpResponseMessage<MobileNumberSignUpKeyModels.GetKeyResponse>(new MobileNumberSignUpKeyModels.GetKeyResponse() {
                Key = signUpKey.SignUpKey.ToString()
            }, HttpStatusCode.Created);
        }

        // PUT /api/mobilenumbersignupkey/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/mobilenumbersignupkey/5
        public void Delete(int id)
        {
        }
    }
}
