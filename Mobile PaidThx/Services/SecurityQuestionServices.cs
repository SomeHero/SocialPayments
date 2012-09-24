using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class SecurityQuestionServices: ServicesBase
    {
        private string _userPayStreamServiceGetUrl = "{0}SecurityQuestions";

        public List<ResponseModels.SecurityQuestionModels.SecurityQuestionResponse> GetSecurityQuestions()
        {
            var response = Get(String.Format(_userPayStreamServiceGetUrl, _webServicesBaseUrl));
            var js = new JavaScriptSerializer();
           
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

             var securityQuestions = js.Deserialize<List<ResponseModels.SecurityQuestionModels.SecurityQuestionResponse>>(response.JsonResponse);

            return securityQuestions;
        }
    }
}