using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using System.Net;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MobileNumberSignUpKeySMSListenerController : ApiController
    {
        private Context _ctx = new Context();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private DomainServices.FormattingServices _formattingServices = new DomainServices.FormattingServices();

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
            _logger.Log(LogLevel.Info, String.Format("Received request for Registration SignUp Key"));

            Guid signUpKeyGuid;
            var signUpKey = request.inboundSMSMessageNotification.inboundSMSMessage.message;
            var mobileNumber = _formattingServices.RemoveFormattingFromMobileNumber(request.inboundSMSMessageNotification.inboundSMSMessage.senderAddress);

            _logger.Log(LogLevel.Info, String.Format("Request details Mobile Number {0}; SignUp Key {1}", mobileNumber, signUpKey));

            Guid.TryParse(signUpKey, out signUpKeyGuid);

            if (signUpKeyGuid == null)
            {
                _logger.Log(LogLevel.Warn, String.Format("Unable to parse Guid {0}", signUpKey));

                return HttpStatusCode.BadRequest;
            }

            var user = _ctx.Users.FirstOrDefault(u => u.UserId.Equals(signUpKeyGuid));

            if (user == null)
            {
                _logger.Log(LogLevel.Warn, String.Format("Unable to find user {0}", signUpKey));

                return HttpStatusCode.BadRequest;
            }

            user.MobileNumber = mobileNumber;

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
