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
        private string _userPayStreamServiceGetUrl = "{0}SecurityQuestions";

        public List<SecurityQuestionModels.SecurityQuestionResponse> GetSecurityQuestions()
        {
            var response = Get(String.Format(_userPayStreamServiceGetUrl, _webServicesBaseUrl));

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(response.Description);

            JavaScriptSerializer js = new JavaScriptSerializer();
            var securityQuestions = js.Deserialize<List<SecurityQuestionModels.SecurityQuestionResponse>>(response.JsonResponse);

            return securityQuestions;
        }
    }
}