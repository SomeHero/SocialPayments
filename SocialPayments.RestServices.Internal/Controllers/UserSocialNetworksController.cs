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
    public class UserSocialNetworksController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
         // POST /api/users/{userId}/SocialNetworks
        [HttpPost]
        public HttpResponseMessage Post(string userId, UserModels.AddUserSocialNetworkRequest request)
        {
            var userSocialNetworkServices = new DomainServices.UserSocialNetworkServices();

            try
            {
                userSocialNetworkServices.AddUserSocialNetwork(userId, request.SocialNetworkType, request.SocialNetworkUserId, request.SocialNetworkUserToken);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Social Network {0} for User {1}.  Exception {1}. Stack Trace {2}", request.SocialNetworkType, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return  Request.CreateResponse(HttpStatusCode.OK);
        }
          // POST /api/users/{userId}/SocialNetworks/unlink
        [HttpPost]
        public HttpResponseMessage UnlinkSocialNetwork(string userId, UserModels.DeleteUserSocialNetworkRequest request)
        {
            var userSocialNetworkServices = new DomainServices.UserSocialNetworkServices();

            try
            {
                userSocialNetworkServices.DeleteUserSocialNetwork(userId, request.SocialNetworkType);

            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Social Network {0} for User {1}.  Exception {1}. Stack Trace {2}", request.SocialNetworkType, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
