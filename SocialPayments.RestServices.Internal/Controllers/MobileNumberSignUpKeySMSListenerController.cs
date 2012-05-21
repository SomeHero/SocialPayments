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
    public class MobileNumberSignUpKeySMSListenerController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/mobilenumbersignupkeysmslistener
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/mobilenumbersignupkeysmslistener/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/mobilenumbersignupkeysmslistener
        public HttpStatusCode Post(MobileNumberSignUpKeySMSListenerModels.UpdateSignUpKeyRequest request)
        {
            Guid signUpKeyGuid;
            var signUpKey = request.inboundSMSMessageNotification.inboundSMSMessage.message;
            var mobileNumber = request.inboundSMSMessageNotification.inboundSMSMessage.senderAddress;

            Guid.TryParse(signUpKey, out signUpKeyGuid);

            if (signUpKeyGuid == null)
                return HttpStatusCode.BadRequest;

            var mobileNumberSignUpKey = _ctx.MobileNumberSignUpKeys.FirstOrDefault(m => m.SignUpKey.Equals(signUpKeyGuid));

            if(mobileNumberSignUpKey == null)
                return HttpStatusCode.BadRequest;

            mobileNumberSignUpKey.MobileNumber = mobileNumber;

            _ctx.SaveChanges();

            return HttpStatusCode.OK;
        }

        // PUT /api/mobilenumbersignupkeysmslistener/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/mobilenumbersignupkeysmslistener/5
        public void Delete(int id)
        {
        }
    }
}
