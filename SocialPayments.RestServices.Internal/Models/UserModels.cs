using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class UserModels
    {
        public class UserResponse
        {
            public Guid userId { get; set; }
            public string userName { get; set; }
            public string deviceToken { get; set; }
            public string emailAddress { get; set; }
            public bool isConfirmed { get; set; }
            public int passwordFailuresSinceLastSuccess { get; set; }
            public DateTime? lastPasswordFailureDate { get; set; }
            public string createDate { get; set; }
            public string mobileNumber { get; set; }
            public bool isLockedOut { get; set; }
            public string timeZone { get; set; }
            public string culture { get; set; }
            public string userStatus { get; set; }
            public string lastLoggedIn { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string imageUrl { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }
            public string senderName { get; set; }
            public double upperLimit { get; set; }
            public string securityQuestion { get; set; }
            public int? securityQuestionId { get; set; }
            public List<UserAttribute> userAttributes { get; set; }
            public string preferredPaymentAccountId { get; set; }
            public string preferredReceiveAccountId { get; set; }
            public string registrationId { get; set; }

            public double totalMoneySent { get; set; }
            public double totalMoneyReceived { get; set; }

            public bool setupSecurityPin { get; set; }
            public int numberOfPaysteamUpdates { get; set; }

            public List<UserPayPointResponse> userPayPoints { get; set; }
            public List<MessageModels.MessageResponse> pendingMessages { get; set; }
            public List<AccountModels.AccountResponse> bankAccounts { get; set; }
            public List<UserModels.UserConfigurationResponse> userConfigurationVariables { get; set; }

        }
        public class SubmitUserRequest
        {
            public string apiKey { get; set; }
            public string userName { get; set; }
            public string password { get; set; }
            public string emailAddress { get; set; }
            public string registrationMethod { get; set; }
            public string deviceToken { get; set; }
        }
        public class SubmitUserResponse
        {
            public string userId { get; set; }
            public string errorResponse { get; set; }
        }
        public class UpdateSecurityPin
        {
            public string securityPin { get; set; }
        }
        public class UpdateSecurityQuestion
        {
            public int questionId { get; set; }
            public string questionAnswer { get; set; }
        }
        public class ChangeSecurityPinRequest
        {
            public string currentSecurityPin { get; set; }
            public string newSecurityPin { get; set; }
        }
        public class UpdateUserRequest
        {

        }
        public class UserAttribute
        {
            public string AttributeName { get; set; }
            public string AttributeValue { get; set; }
        }
        public class ValidateUserRequest
        {
            public string userName { get; set; }
            public string password { get; set; }
        }
        public class ValidateUserResponse
        {
            public bool hasSecurityPin { get; set; }
            public bool hasACHAccount { get; set; }
            public string userId { get; set; }
            public string mobileNumber { get; set; }
            public string paymentAccountId { get; set; }
            public bool setupSecurityPin { get; set; }
            public int upperLimit { get; set; }

            // Added Security Question Implementation
            public bool setupSecurityQuestion { get; set; }
            public bool isLockedOut { get; set; }
        }
        public class FacebookSignInRequest
        {
            public string apiKey { get; set; }
            public string accountId { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string emailAddress { get; set; }
            public string deviceToken { get; set; }
            public string oAuthToken { get; set; }
            //public DateTime tokenExpiration { get; set; }
        }
        public class FacebookSignInResponse
        {
            public bool hasSecurityPin { get; set; }
            public bool hasACHAccount { get; set; }
            public string userId { get; set; }
            public string mobileNumber { get; set; }
            public string paymentAccountId { get; set; }
            public int upperLimit { get; set; }

            // Added Security Question Implementation
            public bool setupSecurityQuestion { get; set; }
            public bool isLockedOut { get; set; }
        }
        public class MECodeResponse
        {
            public string Id { get; set; }
            public string MeCode { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? ApprovedDate { get; set; }
            public bool IsApproved { get; set; }
            public bool IsActive { get; set; }
        }
        public class SubmitMECodeRequest
        {
            public string MeCode { get; set; }
        }
        public class UpdateMECodeRequest
        {
            public DateTime? ApprovedDate { get; set; }
            public bool IsApproved { get; set; }
            public bool IsActive { get; set; }
        }
        public class PersonalizeUserRequest
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ImageUrl { get; set; }
        }
        public class ChangePasswordRequest
        {
            public string currentPassword { get; set; }
            public string newPassword { get; set; }
        }

        public class ForgotPasswordRequest
        {
            public string securityQuestionAnswer { get; set; }
            public string newPassword { get; set; }
        }

        public class ResetPasswordRequest
        {
            public string emailAddress { get; set; }
        }

        public class PushNotificationRequest
        {
            public string registrationId { get; set; }
            public string deviceToken { get; set; }
        }
        public class AddUserPayPointRequest
        {
            public string PayPointType { get; set; }
            public string Uri { get; set; }
        }
        public class UserPayPointResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string Uri { get; set; }
            public string Type { get; set; }
        }
        public class UserConfigurationResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string ConfigurationKey { get; set; }
            public string ConfigurationValue { get; set; }
            public string ConfigurationType { get; set; }
        }
        public class UpdateUserConfigurationRequest
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}