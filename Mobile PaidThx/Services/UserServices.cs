﻿using System;
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
        private string _getMeCodesUrl = "http://23.21.203.171/api/internal/api/users/{0}/mecodes";
        private string _deletePaypointUrl = "http://23.21.203.171/api/internal/api/users/{0}/paypoints/";
        private string _addPaypointUrl = "http://23.21.203.171/api/internal/api/users/{0}/paypoints/";

        public string AddPaypoint(String apiKey, String paypointId, String userId, String uri, String type, Boolean verified, string verifiedDate, string createDate)
        {
            var serviceUrl = String.Format(_addPaypointUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey= apiKey,
                paypointId= paypointId,
                userId= userId, 
                uri= uri,
                type= type,
                verified= verified,
                verifiedDate= verifiedDate, 
                createDate= createDate
            });

            var response = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }


        public ServiceResponse DeletePaypoint(string apiKey, string userId, string paypointId)
        {
            string serviceUrl = String.Format(_deletePaypointUrl, userId) + paypointId;
            var response = Delete(serviceUrl);
            return response;
        }

        public UserModels.UserResponse GetUser(string userId)
        {
            var serviceUrl = String.Format(_userServicesGetBaseUrl, userId);

            var response = Get(serviceUrl);

            JavaScriptSerializer js = new JavaScriptSerializer();

            var user = js.Deserialize<UserModels.UserResponse>(response.JsonResponse);

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
        public ServiceResponse SignInWithFacebook(string apiKey, string accountId, string firstName, string lastName, string emailAddress, string deviceToken, string oAuthToken,
            DateTime tokenExpiration, string messageId)
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
                tokenExpiration = tokenExpiration,
                messageId = messageId
            });

            var response = Post(_userServicesSignInWithFacebookUrl, json);

            return response;

        }
        public string RegisterUser(string serviceUrl, string apiKey, string userName, string password, string emailAddress, string registrationMethod, string deviceToken,
            string messageId)
        {           

            JavaScriptSerializer js = new JavaScriptSerializer();
            
            var json = js.Serialize(new
            {
                apiKey = apiKey,
                userName = userName,
                password = password,
                emailAddress = emailAddress,
                registrationMethod = registrationMethod,
                deviceToken = deviceToken,
                messageId = messageId
            });

            var response = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

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

            var response = Post(serviceUrl, json);

            return response.JsonResponse;
        }
        public string ValidateUser(string userName, string password)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            
            var json = js.Serialize(new
            {
                userName = userName,
                password = password
            });

            var response = Post(_userServiceValidateUserUrl, json);

            return response.JsonResponse;
        }

    }
}