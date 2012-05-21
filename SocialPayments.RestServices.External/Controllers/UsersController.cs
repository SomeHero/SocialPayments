using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.External.Models;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.RestServices.External.Controllers
{
    public class UsersController : ApiController
    {
        private Context _ctx = new Context();

        // GET /api/users
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // GET /api/users/5
        public HttpResponseMessage<UserModels.UserResponse> Get(string id)
        {
            var user = GetUser(id);

            //TODO: check to make sure user exists
            if (user == null)
            {
                var message = new HttpResponseMessage<UserModels.UserResponse>(HttpStatusCode.NotFound);
                message.ReasonPhrase = "User Not Found";

                return message;
            }

            var userResponse = new UserModels.UserResponse()
            {
                Address = user.Address,
                City = user.City,
                createDate = user.CreateDate,
                Culture = user.Culture,
                emailAddress = user.EmailAddress,
                FirstName = user.FirstName,
                isConfirmed = user.IsConfirmed,
                IsLockedOut = user.IsLockedOut,
                LastLoggedIn = user.LastLoggedIn,
                LastName = user.LastName,
                lastPasswordFailureDate = user.LastPasswordFailureDate,
                MobileNumber = user.MobileNumber,
                passwordFailuresSinceLastSuccess = user.PasswordFailuresSinceLastSuccess,
                SenderName = user.SenderName,
                State = user.State,
                TimeZone = user.TimeZone,
                userId = user.UserId,
                userName = user.UserName,
                UserStatus = user.UserStatus.ToString(),
                Zip = user.Zip,
                UserAttributes = user.UserAttributes.Select(a => new UserModels.UserAttribute() {
                     AttributeName = a.UserAttribute.AttributeName,
                     AttributeValue = a.AttributeValue
                }).ToList()
            };

            return new HttpResponseMessage<UserModels.UserResponse>(userResponse, HttpStatusCode.OK);
        }

        // POST /api/users
        public HttpResponseMessage Post(UserModels.SubmitUserRequest request)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // PUT /api/users/5
        public HttpResponseMessage Put(int id, UserModels.UpdateUserRequest request)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/users/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private User GetUser(string id) {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            var user = _ctx.Users
                .Include("UserAttributes")
                .Include("UserAttributes.UserAttribute")
                .FirstOrDefault(u => u.UserId.Equals(userId));

            if (user == null)
                return null;

            return user;
        }
    }
}
