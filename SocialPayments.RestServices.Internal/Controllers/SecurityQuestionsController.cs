using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NLog;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.DomainServices.CustomExceptions;
using SocialPayments.RestServices.Internal.Models;


namespace SocialPayments.RestServices.Internal.Controllers
{
    public class SecurityQuestionsController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/securityquestions
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var securityQuestionServices = new DomainServices.SecurityQuestionServices();
            List<Domain.SecurityQuestion> results = null;

            try
            {
                results = securityQuestionServices.GetSecurityQuestions();
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Security Questions.  Exception {0}.",  ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Security Questions.  Exception {0}.", ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Security Questions.  Exception {0}. Stack Trace {1}",  ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<List<SecurityQuestionModel.QuestionResponse>>(HttpStatusCode.OK, results.Select(q => new SecurityQuestionModel.QuestionResponse()
            {
                Id = q.Id,
                IsActive = q.IsActive,
                Question = q.Question
            }).ToList());
            
        }

        // GET /api/securityquestion/5
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            var securityQuestionServices = new DomainServices.SecurityQuestionServices();
            HttpResponseMessage response = null;
            Domain.SecurityQuestion securityQuestion = null;

            try
            {
                securityQuestion = securityQuestionServices.GetSecurityQuestion(id);

                if (securityQuestion == null)
                    throw new NotFoundException(String.Format("Security Question {0} Not Found", id.ToString()));
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Security Question {0}.  Exception {1}.", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Security Question {0}.  Exception {1}.", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Security Question {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<SecurityQuestionModel.QuestionResponse>(HttpStatusCode.OK, new SecurityQuestionModel.QuestionResponse()
            {
                Id = securityQuestion.Id,
                IsActive = securityQuestion.IsActive,
                Question = securityQuestion.Question
            });

            return response;

        }

        //POST /api/{userId}/validate_security_question
        [HttpPost]
        public HttpResponseMessage ValidateSecurityQuestion(string id, SecurityQuestionModel.ValidateQuestionRequest request)
        {
            var securityQuestionServices = new DomainServices.SecurityQuestionServices();
            bool result = false;

            try
            {
                result = securityQuestionServices.ValidateSecurityQuestion(id, request.questionAnswer);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Validating Security Question for User {0}.  Exception {1}.", id, ex.Message));
                
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Validating Security Question for User {0}.  Exception {1}.", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Validating Security Question for User {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        // POST /api/{userId}/securityquestion
        [HttpPost]
        public HttpResponseMessage Post(string userId, SecurityQuestionModel.SetupQuestionRequest request)
        {
            var securityQuestionServices = new DomainServices.SecurityQuestionServices();

            try
            {
                securityQuestionServices.AddSecurityQuestion(userId, request.questionId, request.questionAnswer);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Security Question Answer for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Security Question Answer for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Security Question Answer {0}. Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        // PUT /api/{userId}/securityquestion
        public HttpResponseMessage Put(string userId, SecurityQuestionModel.UpdateQuestionRequest request)
        {
            //Context _ctx = new Context();
            //DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            //DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

            //User user;

            //// Validate that it finds a user
            //user = _userService.GetUserById(userId);

            //if ( user == null )
            //{
            //    var errorMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            //    errorMessage.ReasonPhrase = String.Format("The user's account cannot be found. {0}", userId);

            //    return errorMessage;
            //}

            //if ( request.questionAnswer.Equals(securityService.Decrypt(user.SecurityQuestionAnswer), StringComparison.OrdinalIgnoreCase)
            //    && request.questionId == user.SecurityQuestionID )
            //{
            //    user.IsLockedOut = false;

            //    _userService.UpdateUser(user);
            //    _ctx.SaveChanges();

            //    return new HttpResponseMessage(HttpStatusCode.OK);
            //}
            //else
            //{
            //    var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            //    errorMessage.ReasonPhrase = String.Format("Invalid security question or answer, please try again.");

            //    return errorMessage;
            //}
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }
    }
}