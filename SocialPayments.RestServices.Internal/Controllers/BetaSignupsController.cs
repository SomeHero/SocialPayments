using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
using System.Net;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class BetaSignupsController : ApiController
    {
        // GET /api/betasignups
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // GET /api/betasignups/5
        public HttpResponseMessage Get(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // POST /api/betasignups
        public HttpResponseMessage Post(Models.BetaSignUpModels.BetaSignUpRequest request)
        {
            using (var ctx = new Context())
            {
                var emailService = new DomainServices.EmailService(ctx);

                var signUp = ctx.BetaSignUps.FirstOrDefault(b => b.EmailAddress == request.EmailAddress);


                if (signUp != null)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = String.Format("{0} is already signed up!", request.EmailAddress);

                    return responseMessage;
                }

                var newSignUp = ctx.BetaSignUps.Add(new Domain.BetaSignup()

                {
                    Id = Guid.NewGuid(),

                    EmailAddress = request.EmailAddress,

                    CreateDate = System.DateTime.Now

                });

                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    responseMessage.ReasonPhrase = String.Format("Error occurred adding beta user. {0}", ex.Message);

                    return responseMessage;
                }

                //Send Email
                try
                {
                    emailService.SendEmail(request.EmailAddress, "Thanks for signing up to be one of the first users of PaidThx!", "Website - Confirmation Sign Up BETA", new List<KeyValuePair<string, string>>());
                }
                catch (Exception ex)
                {
                    //log
                }

                return new HttpResponseMessage(HttpStatusCode.Created);
            }


        }

        // PUT /api/betasignups/5
        public HttpResponseMessage Put(int id, string value)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/betasignups/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }
    }
}
