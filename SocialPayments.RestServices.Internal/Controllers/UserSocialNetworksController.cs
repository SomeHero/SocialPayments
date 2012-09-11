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
        public HttpResponseMessage<UserModels.AddUserSocialNetworkResponse> Post(string userId, UserModels.AddUserSocialNetworkRequest request)
        {
            var userSocialNetworkServices = new DomainServices.UserSocialNetworkServices();
            HttpResponseMessage<UserModels.AddUserSocialNetworkResponse> response = null;


            try
            {
                userSocialNetworkServices.AddUserSocialNetwork(userId, request.SocialNetworkType, request.SocialNetworkUserId, request.SocialNetworkUserToken);

            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Social Network {0} for User {1}.  Exception {1}. Stack Trace {2}", request.SocialNetworkType, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.OK);

            return response;
        }
          // POST /api/users/{userId}/SocialNetworks/unlink
        public HttpResponseMessage UnlinkSocialNetwork(string userId, UserModels.DeleteUserSocialNetworkRequest request)
        {
            var userSocialNetworkServices = new DomainServices.UserSocialNetworkServices();
            HttpResponseMessage<UserModels.AddUserSocialNetworkResponse> response = null;


            try
            {
                userSocialNetworkServices.DeleteUserSocialNetwork(userId, request.SocialNetworkType);

            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Social Network {0} for User {1}.  Exception {2}.", request.SocialNetworkType, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Social Network {0} for User {1}.  Exception {1}. Stack Trace {2}", request.SocialNetworkType, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<UserModels.AddUserSocialNetworkResponse>(HttpStatusCode.OK);

            return response;
        }
    }
}
