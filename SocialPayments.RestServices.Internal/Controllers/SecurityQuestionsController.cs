using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
using NLog;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.RestServices.Internal.Models;


namespace SocialPayments.RestServices.Internal.Controllers
{
    public class SecurityQuestionsController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/securityquestions
        public HttpResponseMessage<List<SecurityQuestionModel.QuestionResponse>> Get()
        {
            Context _ctx = new Context();

            List<SecurityQuestionModel.QuestionResponse> response = new List<SecurityQuestionModel.QuestionResponse>();

            foreach (SecurityQuestion question in _ctx.SecurityQuestions)
            {
                response.Add(new SecurityQuestionModel.QuestionResponse()
                {
                    Id = question.Id,
                    Question = question.Question,
                    IsActive = question.IsActive
                });
            }

            return new HttpResponseMessage<List<SecurityQuestionModel.QuestionResponse>>(response, HttpStatusCode.OK);
        }

        // GET /api/securityquestion/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/{userId}/securityquestion
        public HttpResponseMessage Post(string userId, SecurityQuestionModel.ValidateQuestionRequest request)
        {
            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

            User user;

            // Validate that it finds a user
            user = _userService.GetUserById(userId);

            if ( user == null )
            {
                var errorMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
                errorMessage.ReasonPhrase = String.Format("The user's account cannot be found. {0}", userId);

                return errorMessage;
            }
            if ( user.SecurityQuestionID == null || user.SecurityQuestionAnswer.Length == 0 ) {
                _logger.Log(LogLevel.Info, String.Format("Setting up security question for user {0}, QuestionID {1}", userId, request.questionId));

                if (request.questionId < 0 || request.questionAnswer.Length == 0)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                user.SecurityQuestionAnswer = request.questionAnswer;
                user.SecurityQuestionID = request.questionId;

                return new HttpResponseMessage(HttpStatusCode.Created);
            } else if ( request.questionAnswer.Equals(securityService.Decrypt(user.SecurityQuestionAnswer), StringComparison.OrdinalIgnoreCase)
                && request.questionId == user.SecurityQuestionID )
            {
                user.IsLockedOut = false;

                _userService.UpdateUser(user);
                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                errorMessage.ReasonPhrase = String.Format("Invalid security question or answer, please try again.");

                return errorMessage;
            }
        }

        // PUT /api/{userId}/securityquestion
        public HttpResponseMessage Put(string userId, SecurityQuestionModel.UpdateQuestionRequest request)
        {
            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

            User user;

            // Validate that it finds a user
            user = _userService.GetUserById(userId);

            if ( user == null )
            {
                var errorMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
                errorMessage.ReasonPhrase = String.Format("The user's account cannot be found. {0}", userId);

                return errorMessage;
            }

            if ( request.questionAnswer.Equals(securityService.Decrypt(user.SecurityQuestionAnswer), StringComparison.OrdinalIgnoreCase)
                && request.questionId == user.SecurityQuestionID )
            {
                user.IsLockedOut = false;

                _userService.UpdateUser(user);
                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                errorMessage.ReasonPhrase = String.Format("Invalid security question or answer, please try again.");

                return errorMessage;
            }
        }

        // DELETE /api/{userId}/securityquestion
        public void Delete(int id)
        {
        }
    }
}