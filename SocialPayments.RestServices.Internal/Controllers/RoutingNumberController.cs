using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.ThirdPartyServices.FedACHService;
using System.Net;
using SocialPayments.DomainServices.CustomExceptions;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class RoutingNumberController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        //api/routingnumber/validate
        [HttpPost]
        public HttpResponseMessage ValidateRoutingNumber(RoutingNumberModels.ValidateRoutingNumberRequest request)
        {
            var paymentAccountService = new DomainServices.PaymentAccountService();
            bool results = false;

            try
            {
                results = paymentAccountService.VerifyRoutingNumber(request.RoutingNumber);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Security Questions.  Exception {0}.", ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Security Questions.  Exception {0}.", ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Security Questions.  Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<bool>(HttpStatusCode.OK, results);
            
        }
    }
}
