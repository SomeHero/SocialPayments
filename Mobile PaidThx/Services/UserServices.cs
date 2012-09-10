using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.IO;
using Mobile_PaidThx.Services.ResponseModels;
using NLog;

namespace Mobile_PaidThx.Services
{
    public class UserServices: ServicesBase
    {
        private string _userServiceBaseUrl = "{0}Users";
        private string _userServiceValidateUserUrl = "{0}Users/validate_user";
        private string _userServiceForgotPasswordUrl = "{0}Users/forgot_password";
        private string _userServiceValidateResetPasswordUrl = "{0}Users/validate_passwordreset";
        private string _userServiceChangePasswordUrl = "{0}Users/{1}/change_password";
        private string _userServiceChangeSecurityPinUrl = "{0}Users/{1}/change_securitypin";
        private string _userServicesGetBaseUrl = "{0}Users/{1}";
        private string _userServicesSignInWithFacebookUrl = "{0}users/signin_withfacebook";
        private string _personalizeUrl = "{0}Users/{1}/personalize_user";
        private string _payPointVerificationUrl = "{0}Users/verify_paypoint";
        private string _getMeCodesUrl = "{0}Users/{1}/mecodes";
        private string _userServicesResetSecurityPinUrl = "{0}Users/{1}/setup_securitypin";

        public UserModels.UserResponse GetUser(string userId)
        {
            var js = new JavaScriptSerializer();
            var serviceUrl = String.Format(_userServicesGetBaseUrl, _webServicesBaseUrl, userId);

            var response = Get(serviceUrl);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            return js.Deserialize<UserModels.UserResponse>(response.JsonResponse);

        }

        public UserModels.FacebookSignInResponse SignInWithFacebook(string apiKey, string accountId, string firstName, string lastName, string emailAddress, string deviceToken, string oAuthToken,
            DateTime tokenExpiration, string messageId, out bool isNewUser)
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

            var response = Post(String.Format(_userServicesSignInWithFacebookUrl, _webServicesBaseUrl), json);


            if (response.StatusCode != System.Net.HttpStatusCode.Created && response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(response.Description);

            if(response.StatusCode == HttpStatusCode.Created)
                isNewUser = true;
            else
                isNewUser = false;
    
            return js.Deserialize<UserModels.FacebookSignInResponse>(response.JsonResponse);

        }
        public UserModels.UserResponse RegisterUser(string apiKey, string userName, string password, string emailAddress, string registrationMethod, string deviceToken,
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

            ServiceResponse response = Post(String.Format(_userServiceBaseUrl, _webServicesBaseUrl), json);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new Exception(response.Description);

            return js.Deserialize<UserModels.UserResponse>(response.JsonResponse);
        }
        public void PersonalizeUser(string userId, UserModels.PersonalizeUserRequest request)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var serviceUrl = String.Format(_personalizeUrl, _webServicesBaseUrl, userId);

            var json = js.Serialize(new
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                ImageUrl = request.ImageUrl
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        public void ChangePasssword(string userId, string currentPassword, string newPassword)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                currentPassword = currentPassword,
                newPassword = newPassword
            });

            var response = Post(String.Format(_userServiceChangePasswordUrl, _webServicesBaseUrl, userId), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        public void ForgotPassword(string apiKey, string userName)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey = apiKey,
                userName = userName
            });

            var response = Post(String.Format(_userServiceForgotPasswordUrl, _webServicesBaseUrl), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        public UserModels.ValidateResetPasswordAttemptResponse ValidateResetPasswordAttempt(string apiKey, string id)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                apiKey = apiKey,
                ResetPasswordAttemptId = id
            });

            var response = Post(String.Format(_userServiceValidateResetPasswordUrl, _webServicesBaseUrl), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            return js.Deserialize<UserModels.ValidateResetPasswordAttemptResponse>(response.JsonResponse);
        }
        public void ChangeSecurityPin(string userId, string currentSecurityPin, string newSecurityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                currentSecurityPin = currentSecurityPin,
                newSecurityPin = newSecurityPin
            });

            var response = Post(String.Format(_userServiceChangeSecurityPinUrl, _webServicesBaseUrl, userId), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        public void ResetSecurityPin(string userId, string pinCode, string securityQuestionAnswer)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                securityPin = pinCode,
                questionAnswer = securityQuestionAnswer
            });

            var response = Post(String.Format(_userServicesResetSecurityPinUrl, _webServicesBaseUrl, userId), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        public void ResetPassword(string userId, string pinCode, string securityQuestionAnswer)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                securityPin = pinCode,
                questionAnswer = securityQuestionAnswer
            });

            var response = Post(String.Format(_userServicesResetSecurityPinUrl, _webServicesBaseUrl, userId), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
        public UserModels.ValidateUserResponse ValidateUser(string userName, string password)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            
            var json = js.Serialize(new
            {
                userName = userName,
                password = password
            });

            var response = Post(String.Format(_userServiceValidateUserUrl, _webServicesBaseUrl), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            return js.Deserialize<UserModels.ValidateUserResponse>(response.JsonResponse);
        }

        public void VerifyPayPoint(string payPointVerificationId)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                PayPointVerificationId = payPointVerificationId
            });

            var response = Post(String.Format(_payPointVerificationUrl, _webServicesBaseUrl, payPointVerificationId), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }

    }
}