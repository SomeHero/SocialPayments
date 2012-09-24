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

        // POST /api/mobilenumbersignupkeysmslistener
        [HttpPost]
        public HttpResponseMessage Post(MobileNumberSignUpKeySMSListenerModels.UpdateSignUpKeyRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Received request for Registration SignUp Key"));

            var signUpKey = request.inboundSMSMessageNotification.inboundSMSMessage.message;
            var mobileNumber = _formattingServices.RemoveFormattingFromMobileNumber(request.inboundSMSMessageNotification.inboundSMSMessage.senderAddress);

            _logger.Log(LogLevel.Info, String.Format("Request details Mobile Number {0}; SignUp Key {1}", mobileNumber, signUpKey));

            var userPayPointServices = new DomainServices.UserPayPointServices();

            try
            {
                userPayPointServices.AddMobileNumberSignUp(signUpKey, mobileNumber);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Mobile Number SMS Listener. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
 
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Mobile Number SMS Listener. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Mobile Number SMS Listener. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
  
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
