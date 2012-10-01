using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class UserSecurityQuestionService : ServicesBase
    {
        private string _userValidateSecurityQuestion = "{0}Users/{1}/validate_security_question";

        public bool ValidateSecurityQuestion(string userId, string answer)
        {
            var js = new JavaScriptSerializer();
            var serviceUrl = String.Format(_userValidateSecurityQuestion, _webServicesBaseUrl, userId);

            var json = js.Serialize(new
            {
                questionAnswer = answer
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
            return true;

        }
    }
}