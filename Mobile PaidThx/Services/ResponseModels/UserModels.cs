using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.ResponseModels
{
    public class UserModels
    {
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

            public double totalMoneySent { get; set; }
            public double totalMoneyReceived { get; set; }

            public bool setupSecurityPin { get; set; }
            public int numberOfPaystreamUpdates { get; set; }
            public int newMessageCount { get; set; }
            public int pendingMessageCount { get; set; }

            public List<UserModels.UserPayPointResponse> userPayPoints { get; set; }
            public List<MessageModels.MessageResponse> pendingMessages { get; set; }
            public List<AccountModels.AccountResponse> bankAccounts { get; set; }
            public List<UserModels.UserConfigurationResponse> userConfigurationVariables { get; set; }

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
        public class UserConfigurationResponse
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string ConfigurationKey { get; set; }
            public string ConfigurationValue { get; set; }
            public string ConfigurationType { get; set; }
        }
        public class UserAttribute
        {
            public Guid AttributeId { get; set; }
            public string AttributeName { get; set; }
            public string AttributeValue { get; set; }
        }
        public class UserMeCode
        {
            public String Id { get; set; }
            public String ApprovedDate { get; set; }
            public String CreateDate { get; set; }
            public Boolean IsActive { get; set; }
            public Boolean IsApproved { get; set; }
            public String MeCode { get; set; }
        }
        public class PersonalizeUserRequest
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}