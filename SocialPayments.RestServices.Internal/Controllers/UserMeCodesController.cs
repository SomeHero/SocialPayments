using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using NLog;
using System.Net;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserMeCodesController : ApiController
    {
        private Context _ctx = new Context();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/{userId}/mecodes
        public HttpResponseMessage<List<UserModels.MECodeResponse>> Get(string userId)
        {
           
            Guid id;
            HttpResponseMessage<List<UserModels.MECodeResponse>> message;

            Guid.TryParse(userId, out id);

            if (id == null)
            {
                message = new HttpResponseMessage<List<UserModels.MECodeResponse>>(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid userId {0}", userId);

                return message;
            }

            var meCodes = _ctx.MECodes
                .Where(m => m.UserId == id)
                .Select(m => m)
                .ToList<Domain.MECode>();

            var response = meCodes.Select(m => new UserModels.MECodeResponse() {
                Id = m.Id.ToString(),
                ApprovedDate = m.ApprovedDate,
                CreateDate = m.CreateDate,
                IsActive = m.IsActive,
                IsApproved = m.IsApproved,
                MeCode = m.MeCode
            }).ToList<UserModels.MECodeResponse>();

            return new HttpResponseMessage<List<UserModels.MECodeResponse>>(response, HttpStatusCode.OK);
        }

        // GET /api/{userId}/mecodes/{id}
        public HttpResponseMessage<UserModels.MECodeResponse> Get(string userId, string id)
        {
            Guid userIdGuid;
            Guid idGuid;
            HttpResponseMessage<UserModels.MECodeResponse> message;

            Guid.TryParse(userId, out userIdGuid);

            if (id == null)
            {
                message = new HttpResponseMessage<UserModels.MECodeResponse>(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid userId {0}", userId);

                return message;
            }

            Guid.TryParse(id, out idGuid);

            if (idGuid == null)
            {
                message = new HttpResponseMessage<UserModels.MECodeResponse>(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid MECode Id {0}", id);

                return message;
            }


            var meCode = _ctx.MECodes
                .FirstOrDefault(m => m.UserId == userIdGuid && m.Id == idGuid);

            if (meCode == null)
                return new HttpResponseMessage<UserModels.MECodeResponse>(HttpStatusCode.NotFound);

            var response = new UserModels.MECodeResponse()
            {
                Id = meCode.Id.ToString(),
                ApprovedDate = meCode.ApprovedDate,
                CreateDate = meCode.CreateDate,
                IsActive = meCode.IsActive,
                IsApproved = meCode.IsApproved,
                MeCode = meCode.MeCode
            };

            return new HttpResponseMessage<UserModels.MECodeResponse>(response, HttpStatusCode.OK);

        }

        // POST /api/{userId}/mecodes
        public HttpResponseMessage Post(string userId, UserModels.SubmitMECodeRequest request)
        {
            Guid id;
            HttpResponseMessage message;

            Guid.TryParse(userId, out id);

            if (id == null)
            {
                message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid userId {0}", userId);

                return message;
            }

           Domain.MECode meCode;

           try
           {
               meCode = _ctx.MECodes.Add(new Domain.MECode()
               {
                   CreateDate = System.DateTime.Now,
                   Id = Guid.NewGuid(),
                   IsActive = true,
                   IsApproved = false,
                   UserId = id,
                   MeCode = request.MeCode
               });

               _ctx.SaveChanges();
           }
           catch (Exception ex)
           {
               string errorMessage = String.Format("Exception adding MECode {0} for userId {1}. {2}", request.MeCode, userId, ex.Message);
               _logger.Log(LogLevel.Error, errorMessage);

               message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
               message.ReasonPhrase = errorMessage;

               return message;
           }

           message = new HttpResponseMessage(HttpStatusCode.Created);

           return message;
        }

        // PUT /api/{userId}/mecodes/{id}
        public HttpResponseMessage Put(string userId, string id, UserModels.UpdateMECodeRequest request)
        {
            Guid userIdGuid;
            Guid idGuid;
            HttpResponseMessage message;

            Guid.TryParse(userId, out userIdGuid);

            if (userIdGuid == null)
            {
                message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid userId {0}", userId);

                return message;
            }

            Guid.TryParse(id, out idGuid);

            if(idGuid == null)
            {
                message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid MECode Id {0}", id);

                return message;
            }

            Domain.MECode meCode;

            meCode = _ctx.MECodes
                .FirstOrDefault(m => m.UserId.Equals(userIdGuid) && m.Id.Equals(idGuid));

            if(meCode == null)
            {
                 message = new HttpResponseMessage(HttpStatusCode.NotFound);
                 message.ReasonPhrase = String.Format("Resource {0} Not Found", id);

                return message;
            }
            try
            {
                meCode.IsActive = request.IsActive;
                meCode.LastUpdatedDate = System.DateTime.Now;
                meCode.IsApproved = request.IsApproved;
                meCode.ApprovedDate = request.ApprovedDate;

                _ctx.SaveChanges();
            }
            catch(Exception ex)
            {
                string errorMessage = String.Format("Exception updating MECode {0} for user {1}. {2}", id, userId, ex.Message);

                message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = errorMessage;
                
                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/{userId}mecodes/{id}
        public HttpResponseMessage Delete(string userId, string id)
        {
            Guid userIdGuid;
            Guid idGuid;
            HttpResponseMessage message;

            Guid.TryParse(userId, out userIdGuid);

            if (userIdGuid == null)
            {
                message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid userId {0}", userId);

                return message;
            }

            Guid.TryParse(id, out idGuid);

            if (idGuid == null)
            {
                message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Invalid MECode Id {0}", id);

                return message;
            }

            Domain.MECode meCode;

            meCode = _ctx.MECodes
                .FirstOrDefault(m => m.UserId.Equals(userIdGuid) && m.Id.Equals(idGuid));

            if (meCode == null)
            {
                message = new HttpResponseMessage(HttpStatusCode.OK);
                
                return message;
            }

            try
            {
                _ctx.MECodes.Remove(meCode);

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("Exception Deleting MECode {0} for UserId {1}. {2}", id, userId, ex.Message);

                message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = errorMessage;

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
