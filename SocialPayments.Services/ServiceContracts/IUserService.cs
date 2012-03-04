using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Script.Services;
using SocialPayments.Services.DataContracts.Users;
using SocialPayments.Services.DataContracts;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/UserAcknowledgement", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserAcknowledgementResponse AcknowledgeUser(UserAckowledgementRequest request);

        [OperationContract]
        [WebGet(UriTemplate = "?userName={userName}&password={password}", ResponseFormat = WebMessageFormat.Json)]
        bool ValidateUser(string userName, string password);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Register", BodyStyle= WebMessageBodyStyle.Bare, RequestFormat=WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        UserRegistrationResponse RegisterUser(UserRegistrationRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/VerifyMobileDevice", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserMobileVerificationResponse VerifyMobileDevice(UserMobileVerificationRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SetupPassword", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserSetupPasswordResponse SetupPassword(UserSetupPasswordRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SetupSecurityPin", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserSetupSecurityPinResonse SetupSecurityPin(UserSetupSecurityPinRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SignIn", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserSignInResponse SignInUser(UserSignInRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate= "/ChangeSecurityPin", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserChangeSecurityPinResponse ChangeSecurityPin(UserChangeSecurityPinRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/ForgotPassword", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserForgotPasswordResponse ForgotPassword(UserForgotPasswordRequest request);

        [OperationContract]
        [WebGet(UriTemplate = "/Users", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        List<UserResponse> GetUsers();

        [OperationContract]
        [WebGet(UriTemplate = "/Users/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserResponse GetUser(string id);

        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/Users", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        UserResponse AddUser(UserRequest userRequest);

        [OperationContract]
        [WebInvoke(Method = "Put", UriTemplate = "/Users", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void UpdateUser(UserRequest userRequest);

        [OperationContract]
        [WebInvoke(Method = "Delete", UriTemplate = "/Users", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DeleteUser(UserRequest userRequest);
    }
}