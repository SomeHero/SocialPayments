using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class UserModels
    {
        public class AddUserSocialNetworkRequest
        {
            public string SocialNetworkType { get; set; }
            public string SocialNetworkUserId { get; set; }
            public string SocialNetworkUserToken { get; set; }
        }
        public class AddUserSocialNetworkResponse
        {
            public string SocialNetworkType { get; set; }
            public string SocialNetworkUserId { get; set; }
            public string SocialNetworkUserToken { get; set; }
        }
        public class DeleteUserSocialNetworkRequest
        {
            public string SocialNetworkType { get; set; }
        }
        public class PagedResults
        {
            public int TotalRecords { get; set; }
            public IEnumerable<UserResponse> Results { get; set; }
        }
        public class HomepageRefreshReponse
        {
            public string userId { get; set; }
            public int numberOfIncomingNotifications { get; set; }
            public int numberOfOutgoingNotifications { get; set; }
            public List<UserModels.QuickSendUserReponse> quickSendContacts { get; set; } // Maximum 6 contacts
        }
        public class FindMECodeResponse
        {
            public string searchTerm { get; set; }
            public List<UserModels.MeCodeListResponse> foundUsers { get; set; }
        }
        public class MeCodeListResponse
        {
            public string userId { get; set; }
            public string meCode { get; set; }
        }
        public class QuickSendUserReponse
        {
            public string userUri { get; set; }

            public string userName { get; set; }
            public string userFirstName { get; set; }
            public string userLastName { get; set; }

            public string userImage { get; set; }
            public int userType { get; set; }
        }
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
            public double instantLimit { get; set; }
            public string securityQuestion { get; set; }
            public int? securityQuestionId { get; set; }
            public List<UserAttribute> userAttributes { get; set; }
            public string preferredPaymentAccountId { get; set; }
            public string preferredReceiveAccountId { get; set; }
            public string registrationId { get; set; }

            public string facebookId { get; set; }
            public string facebookToken { get; set; }

            public double totalMoneySent { get; set; }
            public double totalMoneyReceived { get; set; }

            public bool setupSecurityPin { get; set; }
            public int numberOfPaystreamUpdates { get; set; }
            public int newMessageCount { get; set; }
            public int pendingMessageCount { get; set; }
            public double expressDeliveryThreshold { get; set; }
            public double expressDeliveryFeePercentage { get; set; }
            public bool canExpress { get; set; }

            public List<UserPayPointResponse> userPayPoints { get; set; }
            public List<MessageModels.MessageResponse> pendingMessages { get; set; }
            public List<AccountModels.AccountResponse> bankAccounts { get; set; }
            public List<UserModels.UserConfigurationResponse> userConfigurationVariables { get; set; }
            public List<UserSocialNetworkResponse> userSocialNetworks { get; set; }
        }
        public class SubmitUserRequest
        {
            public string userName { get; set; }
            public string password { get; set; }
            public string emailAddress { get; set; }
            public string registrationMethod { get; set; }
            public string deviceToken { get; set; }
            public string messageId { get; set; }
        }
        public class SubmitUserResponse
        {
            public string userId { get; set; }
            public string errorResponse { get; set; }
        }
        public class UpdateSecurityPin
        {
            public string securityPin { get; set; }
            public string questionAnswer { get; set; }
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
            public Guid AttributeId { get; set; }
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
            public string securityQuestion { get; set; }
        }
        public class FacebookSignInRequest
        {
            public string accountId { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string emailAddress { get; set; }
            public string deviceToken { get; set; }
            public string oAuthToken { get; set; }
            public string messageId { get; set; }
            //public DateTime tokenExpiration { get; set; }
        }
        public class ForgotPasswordRequest
        {
            public string userName { get; set; }
        }
        public class LinkFacebookRequest
        {
            public string AccountId { get; set; }
            public string oAuthToken { get; set; }
        }

        public class UnlinkFacebookRequest
        {
        }

        public class FacebookSignInResponse
        {
            public string facebookId { get; set; }
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

        public class ResetPasswordRequest
        {
            public string userId { get; set; }
            public string newPassword { get; set; }
            public string securityQuestionAnswer { get; set; }
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
        public class AddUserPayPointResponse
        {
            public string Id { get; set; }
        }
        public class UserPayPointResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string Uri { get; set; }
            public string Type { get; set; }
            public bool Verified { get; set; }
            public string VerifiedDate { get; set; }
            public string CreateDate { get; set; }
        }
        public class UserSocialNetworkResponse
        {
            public string SocialNetwork { get; set; }
            public string SocialNetworkUserId { get; set; }
            public string SocialNetworkUserToken { get; set; }
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
        public class ResendVerificationCodeRequest
        {
            public string UserPayPointId { get; set; }
        }
        public class ValidatePasswordResetAttemptRequest
        {
            public string ResetPasswordAttemptId { get; set; }
        }
        public class ValidatePasswordResetAttemptResponse
        {
            public bool HasSecurityQuestion { get; set; }
            public string SecurityQuestion { get; set; }
            public string UserId { get; set; }
        }
        public class SendEmailRequest
        {
            public string Subject { get; set; }
            public string TemplateName { get; set; }
            public List<KeyValuePair<string, string>> ReplacementElements { get; set; }
        }
        public class UpdateUserAttributeRequest
        {
            public string AttributeValue { get; set; }
        }
        public class UpdateUserAttributeResponse
        {
            public string AttributeKey { get; set; }
            public string AttributeValue { get; set; }
        }
        public class ValidatePayPointRequest
        {
            public Guid PayPointVerificationId { get; set; }
        }
        public class VerifyMobilePayPointRequest
        {
            public string VerificationCode { get; set; }
        }
        public class VerifyMobilePayPointResponse
        {
            public bool Verified { get; set; }
        }
        public class ValidateSecurityPinRequest
        {
            public string SecurityPin { get; set; }
        }
        public class ValidateSecurityPinResponse
        {
            public bool IsValid { get; set; }
        }
    }
}