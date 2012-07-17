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
                        Uri = p.URI
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
        public HttpResponseMessage Post(string userId, Models.UserModels.AddUserPayPointRequest request)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                {
                    var response = 
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("User {0} not found", userId);

                    return response;
                }
                var payPointType = _ctx.PayPointTypes.FirstOrDefault(p => p.Name == request.PayPointType);

                if (payPointType == null)
                {
                    var response =
                       Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("Pay Point Type {0} not found", request.PayPointType);

                    return response;
                }

                //TODO: Validate format of the URI based on type

                user.PayPoints.Add(new UserPayPoint()
                {
                    Id = Guid.NewGuid(),
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    URI = request.Uri,
                    Type = payPointType
                });

                userService.UpdateUser(user);

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
        }

        // PUT /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage Put(string userId)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/users/{userId}/PayPoints/{id}
        public HttpResponseMessage Delete(string userId, int id)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                {
                    var response =
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("User {0} not found", userId);

                    return response;
                }

                var payPoint = _ctx.UserPayPoints
                    .FirstOrDefault(p => p.UserId == user.UserId);

                if(payPoint == null)
                {
                    var response =
                        Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = String.Format("PayPoint {0} not found for specified user {1}", id, userId);

                    return response;
                }

                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
    }
}