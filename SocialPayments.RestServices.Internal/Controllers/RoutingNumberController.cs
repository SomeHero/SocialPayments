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
        public HttpResponseMessage<bool> ValidateRoutingNumber(RoutingNumberModels.ValidateRoutingNumberRequest request)
        {
            HttpResponseMessage<bool> response = null;
            bool results = false;
            try
            {

                FedACHService fedACHService = new FedACHService();

                FedACHList fedACHList = new FedACHList();
                //results = fedACHService.getACHByRoutingNumber(request.RoutingNumber, out fedACHList);
                results = true;
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Security Questions.  Exception {0}.", ex.Message));

                response = new HttpResponseMessage<bool>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Security Questions.  Exception {0}.", ex.Message));

                response = new HttpResponseMessage<bool>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Security Questions.  Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<bool>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            return new HttpResponseMessage<bool>(results);
            
        }
    }
}
