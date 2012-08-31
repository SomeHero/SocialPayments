using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.Domain;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DomainServices;
using System.Net;
using System.Data.Entity;
using SocialPayments.RestServices.Internal.Controllers;

namespace SocialPayments.RestServices.Internal.Controllers.Controllers
{
    public class UserPayPointController : ApiController
    {
        // GET /api/users/{userId}/PayPoints/
        public HttpResponseMessage<List<UserModels.UserPayPointResponse>> Get(string userId)
        {
            return Get(userId, "");
        }
        // GET /api/users/{userId}/PayPoints/{type}
        public HttpResponseMessage<List<UserModels.UserPayPointResponse>> Get(string userId, string type)
        {
            var userPayPointServices = new UserPayPointServices();
            List<Domain.UserPayPoint> userPayPoints = null;
            HttpResponseMessage<List<UserModels.UserPayPointResponse>> response = null;

            try
            {
                userPayPoints = userPayPointServices.GetUserPayPoints(userId, type);
            }
            catch (Exception ex)
            {

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
            var userPayPointServices = new UserPayPointServices();
            Domain.UserPayPoint payPoint = null;
            HttpResponseMessage<UserModels.UserPayPointResponse> response = null;

            try
            {
                payPoint = userPayPointServices.GetUserPayPoint(userId, type);
            }
            catch (Exception ex)
            {

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
            var userPayPointServices = new UserPayPointServices();
            HttpResponseMessage<UserModels.AddUserPayPointResponse> response = null;
            Domain.UserPayPoint userPayPoint = null;

            try
            {
                userPayPoint = userPayPointServices.AddUserPayPoint(userId, request.PayPointType, request.Uri);
            }
            catch (Exception ex)
            {

            }

            //if (payPointType.Name == "EmailAddress")
            //    userService.SendEmailVerificationLink(userPayPoint);
            //else if (payPointType.Name == "Phone")
            //    userService.SendMobileVerificationCode(userPayPoint);

            response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(new UserModels.AddUserPayPointResponse()
            {
                Id = userPayPoint.Id.ToString()
            }, HttpStatusCode.Created);

            return response;
        }
        // POST /api/users/{userId}/PayPoints/resend_verification_code
        public HttpResponseMessage ResendVerificationCode(string userId, UserModels.ResendVerificationCodeRequest model)
        {
            DomainServices.UserPayPointServices userPayPointService = new UserPayPointServices();
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
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;

        }
        // POST /api/users/{userId}/PayPoints/resend_email_verification_link
        public HttpResponseMessage ResendEmailVerificationLink(string userId, UserModels.ResendVerificationCodeRequest model)
        {
                DomainServices.UserPayPointServices userPayPointService = new UserPayPointServices();
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
                catch (Exception ex)
                {

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
            DomainServices.UserPayPointServices userPayPointService = new UserPayPointServices();
            HttpResponseMessage response = null;

            try
            {
                userPayPointService.DeleteUserPayPoint(userId, id);
            }
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }
        // api/users/{userId}/PayPoints/{id}/verify_mobile_paypoint
        [HttpPost]
        public HttpResponseMessage<UserModels.VerifyMobilePayPointResponse> VerifyMobilePayPoint(string userId, string id, UserModels.VerifyMobilePayPointRequest request)
        {
            Guid userGuid;
            Guid userPayPointGuid;

            var userService = new DomainServices.UserService();

            HttpResponseMessage<UserModels.VerifyMobilePayPointResponse> response;

            bool results = false;

            try
            {
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new ArgumentException("Invalid User Specified");
                
                Guid.TryParse(id, out userPayPointGuid);

                if (userPayPointGuid == null)
                    throw new ArgumentException("Invalid Pay Point Specified");

                results = userService.VerifyMobilePayPoint(userGuid, userPayPointGuid, request.VerificationCode);

            }
            catch (Exception ex)
            {
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