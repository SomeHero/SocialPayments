using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
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
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);

                List<Domain.UserPayPoint> payPoints;

                if (!String.IsNullOrEmpty(type))
                {
                    int typeId = 0;

                    if (type == "EmailAddress")
                        typeId = 1;
                    else if (type == "Phone")
                        typeId = 2;
                    else if (type == "MeCode")
                        typeId = 4;

                    payPoints = _ctx.UserPayPoints
                    .Include("Type")
                    .Where(p => p.UserId == user.UserId && p.IsActive  && p.PayPointTypeId == typeId)
                    .Select(p => p)
                    .ToList<Domain.UserPayPoint>();
                }
                else
                {
                    payPoints = _ctx.UserPayPoints
                    .Include("Type")
                    .Where(p => p.UserId == user.UserId && p.IsActive)
                    .Select(p => p)
                    .ToList<Domain.UserPayPoint>();
                }

                return new HttpResponseMessage<List<UserModels.UserPayPointResponse>>(payPoints.Select(p =>
                    new UserModels.UserPayPointResponse() {
                        Id = p.Id.ToString(),
                        UserId = p.UserId.ToString(),
                        Type = p.Type.Name,
                        Uri = p.URI,
                        Verified = p.Verified
                    }).ToList(), HttpStatusCode.OK);

            }
        }

        // GET /api/users/{userId}/PayPoints/{id}
        public UserModels.UserPayPointResponse Get(string userId, string id, string type)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);
                Guid payPointId;

                Guid.TryParse(id, out payPointId);

                var payPoint = _ctx.UserPayPoints
                    .Include("Type")
                    .FirstOrDefault(p => p.UserId == user.UserId && p.Id == payPointId);

                return new UserModels.UserPayPointResponse()
                {
                    Id = payPoint.Id.ToString(),
                    UserId = payPoint.UserId.ToString(),
                    Type = payPoint.Type.ToString(),
                    Uri = payPoint.URI
                };
            }
        }

        // POST /api/users/{userId}/PayPoints
        public HttpResponseMessage<UserModels.AddUserPayPointResponse> Post(string userId, Models.UserModels.AddUserPayPointRequest request)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);
                HttpResponseMessage<UserModels.AddUserPayPointResponse> response;

                var user = userService.GetUserById(userId);

                if (user == null)
                {
                    response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("User {0} not found", userId);

                    return response;
                }
                var payPointType = _ctx.PayPointTypes.FirstOrDefault(p => p.Name == request.PayPointType);

                if (payPointType == null)
                {
                    response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("Pay Point Type {0} not found", request.PayPointType);

                    return response;
                }

                var payPoints = _ctx.UserPayPoints.FirstOrDefault(p => p.URI == request.Uri);

                if (payPoints != null)
                {

                    response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("The pay point is already linked to an account.", request.PayPointType);

                    return response;
                }

                //TODO: Validate format of the URI based on type

                var userPayPoint = _ctx.UserPayPoints.Add(new UserPayPoint()
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    URI = request.Uri,
                    Type = payPointType,
                    Verified = false
                });

                if(payPointType.Name == "EmailAddress")
                    userService.SendEmailVerificationLink(userPayPoint);
                else if(payPointType.Name == "Phone")
                    userService.SendMobileVerificationCode(userPayPoint);

                userService.UpdateUser(user);

                response = new HttpResponseMessage<UserModels.AddUserPayPointResponse>(new UserModels.AddUserPayPointResponse() {
                    Id = userPayPoint.Id.ToString()
                }, HttpStatusCode.Created);

                return response;
            }
        }
        // POST /api/users/{userId}/PayPoints/resend_verification_code
        public HttpResponseMessage ResendVerificationCode(string userId, UserModels.ResendVerificationCodeRequest model)
        {
            using (var ctx = new Context())
            {
                UserService userService = new UserService(ctx);

                Guid userPayPointId;

                Guid.TryParse(model.UserPayPointId, out userPayPointId);

                if(userPayPointId == null)
                {
                    var response = 
                        new HttpResponseMessage(HttpStatusCode.NotFound);
                    response.ReasonPhrase = String.Format("User PayPoint {0} not found", model.UserPayPointId);

                    return response;
                }

                var userPayPoint = ctx.UserPayPoints.FirstOrDefault(p => p.Id == userPayPointId);

                try
                {
                    userService.SendMobileVerificationCode(userPayPoint);
                }
                catch (Exception ex)
                {
                    var response =
                                            new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    response.ReasonPhrase = "Error occcurred resending verification code";

                    return response;
                }

                return new HttpResponseMessage(HttpStatusCode.OK);

            }
        }
        // POST /api/users/{userId}/PayPoints/resend_email_verification_link
        public HttpResponseMessage ResendEmailVerificationLink(string userId, UserModels.ResendVerificationCodeRequest model)
        {
            using (var ctx = new Context())
            {
                UserService userService = new UserService(ctx);

                Guid userPayPointId;

                Guid.TryParse(model.UserPayPointId, out userPayPointId);

                if (userPayPointId == null)
                {
                    var response =
                        new HttpResponseMessage(HttpStatusCode.NotFound);
                    response.ReasonPhrase = String.Format("User PayPoint {0} not found", model.UserPayPointId);

                    return response;
                }

                var userPayPoint = ctx.UserPayPoints.FirstOrDefault(p => p.Id == userPayPointId);

                try
                {
                    userService.SendEmailVerificationLink(userPayPoint);
                }
                catch (Exception ex)
                {
                    var response =  new HttpResponseMessage(HttpStatusCode.InternalServerError);

                    response.ReasonPhrase = "Error occcurred resending verification code";

                    return response;
                }

                return new HttpResponseMessage(HttpStatusCode.OK);

            }
        }
        // PUT /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage Put(string userId)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage Delete(string userId, string id)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                Guid userIdGuid;
                Guid.TryParse(userId, out userIdGuid);

                if (userIdGuid == null)
                {
                    var response =
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("User Id {0} is invalid", userId);

                    return response;
                }

                Guid payPointId;
                Guid.TryParse(id, out payPointId);

                if (payPointId == null)
                {
                    var response =
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("PayPoint Id {0} is invalid", payPointId);

                    return response;
                }

                var user = userService.GetUserById(userId);

                if (user == null)
                {
                    var response =
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("User {0} not found", userId);

                    return response;
                }

                var payPoint = _ctx.UserPayPoints
                    .FirstOrDefault(p => p.Id == payPointId  && p.UserId == userIdGuid);

                if(payPoint == null)
                {
                    var response =
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("PayPoint {0} not found for specified user {1}", id, userId);

                    return response;
                }

                payPoint.IsActive = false;
                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
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