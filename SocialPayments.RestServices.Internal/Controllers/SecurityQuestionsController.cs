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

        // GET /api/securityquestion
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

        // POST /api/securityquestion
        public void Post(string value)
        {
        }

        // PUT /api/securityquestion/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/securityquestion/5
        public void Delete(int id)
        {
        }
    }
}