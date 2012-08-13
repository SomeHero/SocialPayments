using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.IO;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Services
{
    public class UserServices: ServicesBase
    {
        private string _userServiceBaseUrl = "http://23.21.203.171/api/internal/api/Users";
        private string _userServiceValidateUserUrl = "http://23.21.203.171/api/internal/api/Users/validate_user";
        private string _userServicesGetBaseUrl = "http://23.21.203.171/api/internal/api/Users/{0}";
        private string _userServicesSignInWithFacebookUrl = "http://23.21.203.171/api/internal/api/users/signin_withfacebook";
        private string _personalizeUrl = "http://23.21.203.171/api/internal/api/users/{0}/personalize_user";

        public UserModels.UserResponse GetUser(string userId)
        {
            var serviceUrl = String.Format(_userServicesGetBaseUrl, userId);

            var jsonResponse = Get(serviceUrl);

            JavaScriptSerializer js = new JavaScriptSerializer();

            var user = js.Deserialize<UserModels.UserResponse>(jsonResponse);

            return user;
        }
        //        public class FacebookSignInRequest
        //{
        //    public string apiKey { get; set; }
        //    public string accountId { get; set; }
        //    public string firstName { get; set; }
        //    public string lastName { get; set; }
        //    public string emailAddress { get; set; }
        //    public string deviceToken { get; set; }
        //    public string oAuthToken { get; set; }
        //    //public DateTime tokenExpiration { get; set; }
        //}
        public string SignInWithFacebook(string apiKey, string accountId, string firstName, string lastName, string emailAddress, string deviceToken, string oAuthToken,
            DateTime tokenExpiration)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey = apiKey,
                accountId = accountId,
                firstName = firstName,
                lastName = lastName,
                emailAddress = emailAddress,
                deviceToken = deviceToken,
                tokenExpiration = tokenExpiration
            });

            string jsonResponse = Post(_userServicesSignInWithFacebookUrl, json);

            return jsonResponse;

        }
        public string RegisterUser(string serviceUrl, string apiKey, string userName, string password, string emailAddress, string registrationMethod, string deviceToken)
        {           

            JavaScriptSerializer js = new JavaScriptSerializer();
            
            var json = js.Serialize(new
            {
                apiKey = apiKey,
                userName = userName,
                password = password,
                emailAddress = emailAddress,
                registrationMethod = registrationMethod,
                deviceToken = deviceToken
            });

            string jsonResponse = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(jsonResponse);

            return jsonObject["userId"];
        }
        public string PersonalizeUser(string userId, UserModels.PersonalizeUserRequest request)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var serviceUrl = String.Format(_personalizeUrl, userId);

            var json = js.Serialize(new
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                ImageUrl = request.ImageUrl
            });

            string jsonResponse = Post(serviceUrl, json);

            return jsonResponse;
        }
        public string ValidateUser(string userName, string password)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            
            var json = js.Serialize(new
            {
                userName = userName,
                password = password
            });

            string jsonResponse = Post(_userServiceValidateUserUrl, json);

            return jsonResponse;
        }

    }
}