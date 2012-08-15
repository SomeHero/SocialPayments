using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{
    public class SecurityQuestionServices: ServicesBase
    {
        private string _userPayStreamServiceGetUrl = "http://23.21.203.171/api/internal/api/SecurityQuestions";

        public List<SecurityQuestionModels.SecurityQuestionResponse> GetSecurityQuestions()
        {
            var response = Get(_userPayStreamServiceGetUrl);

            JavaScriptSerializer js = new JavaScriptSerializer();

            var securityQuestions = js.Deserialize<List<SecurityQuestionModels.SecurityQuestionResponse>>(response.JsonResponse);

            return securityQuestions;
        }
    }
}