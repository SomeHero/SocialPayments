using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.Domain;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DomainServices.CustomExceptions;
using System.Net;
using System.Data.Entity;
using SocialPayments.RestServices.Internal.Controllers;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers.Controllers
{
    public class UserPayPointController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        // GET /api/users/{userId}/PayPoints/
        public HttpResponseMessage<List<UserModels.UserPayPointResponse>> Get(string userId)
        {
            return Get(userId, "");
        }
        // GET /api/users/{userId}/PayPoints/{type}
        public HttpResponseMessage<List<UserModels.UserPayPointResponse>> Get(string userId, string type)
        {
            var userPayPointServices = new DomainServices.UserPayPointServices();
            List<Domain.UserPayPoint> userPayPoints = null;
            HttpResponseMessage<List<UserModels.UserPayPointResponse>> response = null;

            try
            {
                userPayPoints = userPayPointServices.GetUserPayPoints(userId, type);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Pay Points for User {0}.  Exception {1}.", userId, ex.Message));

                response = new HttpResponseMessage<List<UserModels.UserPayPointResponse>>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Pay Points for User {0}.  Exception {1}.", userId, ex.Message));

                response = new HttpResponseMessage<List<UserModels.UserPayPointResponse>>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Pay Points for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<UserModels.UserPayPointResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<List<UserModels.UserPayPointResponse>>(userPayPoints.Select(p =>
                    new UserModels.UserPayPointResponse()
                    {
                        Id = p.Id.ToString(),
                        UserId = p.UserId.ToString(),
                        Type = p.Type.Name,
                        Uri = p.URI,
                        Verified = p.Verified
                    }).ToList(), HttpStatusCode.OK);

            return response;

        }

        // GET /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage<UserModels.UserPayPointResponse> Get(string userId, string id, string type)
        {
            var userPayPointServices = new DomainServices.UserPayPointServices();
            Domain.UserPayPoint payPoint = null;
            HttpResponseMessage<UserModels.UserPayPointResponse> response = null;

            try
            {
                payPoint = userPayPointServices.GetUserPayPoint(userId, type);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.UserPayPointResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.UserPayPointResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Pay Point {0} for User {1}.  Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.UserPayPointResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<UserModels.UserPayPointResponse>(new UserModels.UserPayPointResponse() 
            {
                Id = payPoint.Id.ToString(),
                UserId = payPoint.UserId.ToString(),
                Type = payPoint.Type.ToString(),
                Uri = payPoint.URI
            }, HttpStatusCode.OK);

            return response;

        }

        // POST /api/users/{userId}/PayPoints
        public HttpResponseMessage<UserModels.AddUserPayPointResponse> Post(string userId, Models.UserModels.AddUserPayPointRequest request)
        {
            var userPayPointServices = new DomainServices.UserPayPointServices();
            HttpResponseMessage<UserModels.AddUserPayPointResponse> response = null;
            Domain.UserPayPoint userPayPoint = null;

            try
            {
                userPayPoint = userPayPointServices.AddUserPayPoint(userId, request.PayPointType, request.Uri);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Pay Point {0} for User {1}.  Exception {2}.", request.Uri, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Pay Point {0} for User {1}.  Exception {2}.", request.Uri, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Pay Point {0} for User {1}.  Exception {1}. Stack Trace {2}", request.Uri, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(new UserModels.AddUserPayPointResponse()
            {
                Id = userPayPoint.Id.ToString()
            }, HttpStatusCode.Created);

            return response;
        }
        // POST /api/users/{userId}/PayPoints/resend_verification_code
        public HttpResponseMessage ResendVerificationCode(string userId, UserModels.ResendVerificationCodeRequest model)
        {
            DomainServices.UserPayPointServices userPayPointService = new DomainServices.UserPayPointServices();
            DomainServices.UserService userServices = new DomainServices.UserService();
            Domain.UserPayPoint userPayPoint = null;
            HttpResponseMessage response = null;

            try
            {
                userPayPoint = userPayPointService.GetUserPayPoint(userId, model.UserPayPointId);

                if (userPayPoint == null)
                    throw new SocialPayments.DomainServices.CustomExceptions.NotFoundException(String.Format("User Pay Point {0} Not Found", model.UserPayPointId));
                    
                userServices.SendMobileVerificationCode(userPayPoint);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Resending Verification Code {0} for User {1}.  Exception {2}.", model.UserPayPointId, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Resending Verification Code {0} for User {1}.  Exception {2}.", model.UserPayPointId, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Resending Verification Code {0} for User {1}.  Exception {1}. Stack Trace {2}", model.UserPayPointId, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;

        }
        // POST /api/users/{userId}/PayPoints/resend_email_verification_link
        public HttpResponseMessage ResendEmailVerificationLink(string userId, UserModels.ResendVerificationCodeRequest model)
        {
                DomainServices.UserPayPointServices userPayPointService = new DomainServices.UserPayPointServices();
                DomainServices.UserService userServices = new DomainServices.UserService();
                Domain.UserPayPoint userPayPoint = null;
                HttpResponseMessage response = null;

                try
                {
                    userPayPoint = userPayPointService.GetUserPayPoint(userId, model.UserPayPointId);

                    if (userPayPoint == null)
                        throw new SocialPayments.DomainServices.CustomExceptions.NotFoundException(String.Format("User Pay Point {0} Not Found", model.UserPayPointId));

                    userServices.SendEmailVerificationLink(userPayPoint);
                }
                catch (NotFoundException ex)
                {
                    _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Resending Verification Link {0} for User {1}.  Exception {2}.", model.UserPayPointId, userId, ex.Message));

                    response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.NotFound);
                    response.ReasonPhrase = ex.Message;

                    return response;
                }
                catch (BadRequestException ex)
                {
                    _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Resending Verification Link {0} for User {1}.  Exception {2}.", model.UserPayPointId, userId, ex.Message));

                    response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = ex.Message;

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Resending Verification Link {0} for User {1}.  Exception {1}. Stack Trace {2}", model.UserPayPointId, userId, ex.Message, ex.StackTrace));

                    response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.InternalServerError);
                    response.ReasonPhrase = ex.Message;

                    return response;
                }

                response = new HttpResponseMessage(HttpStatusCode.OK);

                return response;

        }
        // PUT /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage Put(string userId)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage Delete(string userId, string id)
        {
            DomainServices.UserPayPointServices userPayPointService = new DomainServices.UserPayPointServices();
            HttpResponseMessage response = null;

            try
            {
                userPayPointService.DeleteUserPayPoint(userId, id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Pay Point {0} for User {1}.  Exception {1}. Stack Trace {2}", id, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }
        // api/users/{userId}/PayPoints/{id}/verify_mobile_paypoint
        [HttpPost]
        public HttpResponseMessage<UserModels.VerifyMobilePayPointResponse> VerifyMobilePayPoint(string userId, string id, UserModels.VerifyMobilePayPointRequest request)
        {
            var userService = new DomainServices.UserService();
            HttpResponseMessage<UserModels.VerifyMobilePayPointResponse> response;
            bool results = false;

            try
            {
                results = userService.VerifyMobilePayPoint(userId, id, request.VerificationCode);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Verifying Mobile Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.VerifyMobilePayPointResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Verifying Mobile Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                response = new HttpResponseMessage<UserModels.VerifyMobilePayPointResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Verifying Mobile Pay Point {0} for User {1}.  Exception {1}. Stack Trace {2}", id, userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.VerifyMobilePayPointResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<UserModels.VerifyMobilePayPointResponse>(new UserModels.VerifyMobilePayPointResponse() {
                Verified = results
            }, HttpStatusCode.OK);


            return response;
        }

    }
}