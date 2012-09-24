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
        [HttpGet]
        public HttpResponseMessage Get(string userId)
        {
            return Get(userId, "");
        }

        // GET /api/users/{userId}/PayPoints/{type}
        [HttpGet]
        public HttpResponseMessage Get(string userId, string type)
        {
            var userPayPointServices = new DomainServices.UserPayPointServices();
            List<Domain.UserPayPoint> userPayPoints = null;

            try
            {
                userPayPoints = userPayPointServices.GetUserPayPoints(userId, type);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Pay Points for User {0}.  Exception {1}.", userId, ex.Message));
                
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Pay Points for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Pay Points for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<List<UserModels.UserPayPointResponse>>(HttpStatusCode.OK, userPayPoints.Select(p =>
                    new UserModels.UserPayPointResponse()
                    {
                        Id = p.Id.ToString(),
                        UserId = p.UserId.ToString(),
                        Type = p.Type.Name,
                        Uri = p.URI,
                        Verified = p.Verified
                    }).ToList());
        }

        // GET /api/users/{userId}/PayPoints/{id}
        [HttpGet]
        public HttpResponseMessage Get(string userId, string id, string type)
        {
            var userPayPointServices = new DomainServices.UserPayPointServices();
            Domain.UserPayPoint payPoint = null;

            try
            {
                payPoint = userPayPointServices.GetUserPayPoint(userId, type);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Pay Point {0} for User {1}.  Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new UserModels.UserPayPointResponse() 
            {
                Id = payPoint.Id.ToString(),
                UserId = payPoint.UserId.ToString(),
                Type = payPoint.Type.ToString(),
                Uri = payPoint.URI
            });

        }

        // POST /api/users/{userId}/PayPoints
        [HttpPost]
        public HttpResponseMessage Post(string userId, Models.UserModels.AddUserPayPointRequest request)
        {
            var userPayPointServices = new DomainServices.UserPayPointServices();
            Domain.UserPayPoint userPayPoint = null;

            try
            {
                userPayPoint = userPayPointServices.AddUserPayPoint(userId, request.PayPointType, request.Uri);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Pay Point {0} for User {1}.  Exception {2}.", request.Uri, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Pay Point {0} for User {1}.  Exception {2}.", request.Uri, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Pay Point {0} for User {1}.  Exception {1}. Stack Trace {2}", request.Uri, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }

            return Request.CreateResponse<UserModels.AddUserPayPointResponse>(HttpStatusCode.Created, new UserModels.AddUserPayPointResponse()
            {
                Id = userPayPoint.Id.ToString()
            });
        }
        // POST /api/users/{userId}/PayPoints/resend_verification_code
        [HttpPost]
        public HttpResponseMessage ResendVerificationCode(string userId, UserModels.ResendVerificationCodeRequest model)
        {
            DomainServices.UserPayPointServices userPayPointService = new DomainServices.UserPayPointServices();
            DomainServices.UserService userServices = new DomainServices.UserService();
            Domain.UserPayPoint userPayPoint = null;

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

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Resending Verification Code {0} for User {1}.  Exception {2}.", model.UserPayPointId, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Resending Verification Code {0} for User {1}.  Exception {1}. Stack Trace {2}", model.UserPayPointId, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        // POST /api/users/{userId}/PayPoints/resend_email_verification_link
        [HttpPost]
        public HttpResponseMessage ResendEmailVerificationLink(string userId, UserModels.ResendVerificationCodeRequest model)
        {
                DomainServices.UserPayPointServices userPayPointService = new DomainServices.UserPayPointServices();
                DomainServices.UserService userServices = new DomainServices.UserService();
                Domain.UserPayPoint userPayPoint = null;

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

                    var error = new HttpError(ex.Message);

                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            
                }
                catch (BadRequestException ex)
                {
                    _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Resending Verification Link {0} for User {1}.  Exception {2}.", model.UserPayPointId, userId, ex.Message));

                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Resending Verification Link {0} for User {1}.  Exception {1}. Stack Trace {2}", model.UserPayPointId, userId, ex.Message, ex.StackTrace));

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

                return Request.CreateResponse(HttpStatusCode.OK);

        }
        // PUT /api/users/{userId}/PayPoints/{id}
        [HttpPut]
        public HttpResponseMessage Put(string userId)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/users/{userId}/PayPoints/{id}
        [HttpDelete]
        public HttpResponseMessage Delete(string userId, string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Deleting Pay Point {0} for User {1}.", id, userId));

            DomainServices.UserPayPointServices userPayPointService = new DomainServices.UserPayPointServices();

            try
            {
                userPayPointService.DeleteUserPayPoint(userId, id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Pay Point {0} for User {1}.  Exception {1}. Stack Trace {2}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        // api/users/{userId}/PayPoints/{id}/verify_mobile_paypoint
        [HttpPost]
        public HttpResponseMessage VerifyMobilePayPoint(string userId, string id, UserModels.VerifyMobilePayPointRequest request)
        {
            var userService = new DomainServices.UserService();
            bool results = false;

            try
            {
                results = userService.VerifyMobilePayPoint(userId, id, request.VerificationCode);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Verifying Mobile Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Verifying Mobile Pay Point {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Verifying Mobile Pay Point {0} for User {1}.  Exception {1}. Stack Trace {2}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            return Request.CreateResponse<UserModels.VerifyMobilePayPointResponse>(HttpStatusCode.OK, new UserModels.VerifyMobilePayPointResponse() {
                Verified = results
            });
        }

    }
}