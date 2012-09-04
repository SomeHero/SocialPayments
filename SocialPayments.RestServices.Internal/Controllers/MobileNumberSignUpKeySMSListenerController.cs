using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using NLog;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MobileNumberSignUpKeySMSListenerController : ApiController
    {
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
        public HttpResponseMessage Post(MobileNumberSignUpKeySMSListenerModels.UpdateSignUpKeyRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Received request for Registration SignUp Key"));

            var signUpKey = request.inboundSMSMessageNotification.inboundSMSMessage.message;
            var mobileNumber = _formattingServices.RemoveFormattingFromMobileNumber(request.inboundSMSMessageNotification.inboundSMSMessage.senderAddress);

            _logger.Log(LogLevel.Info, String.Format("Request details Mobile Number {0}; SignUp Key {1}", mobileNumber, signUpKey));

            var userPayPointServices = new DomainServices.UserPayPointServices();
            HttpResponseMessage response = null;

            try
            {
                userPayPointServices.AddMobileNumberSignUp(signUpKey, mobileNumber);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Mobile Number SMS Listener. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Mobile Number SMS Listener. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Mobile Number SMS Listener. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;

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
