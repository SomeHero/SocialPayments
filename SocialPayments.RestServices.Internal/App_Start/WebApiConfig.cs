using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Routing;

namespace SocialPayments
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            config.Routes.MapHttpRoute(
               name: "UnlinkSocialNetwork",
               routeTemplate: "api/users/{userId}/SocialNetworks/unlink",
               defaults: new { controller = "UserSocialNetworks", action = "UnlinkSocialNetwork" }
            );
            config.Routes.MapHttpRoute(
               name: "SocialNetworks",
               routeTemplate: "api/users/{userId}/socialnetworks",
               defaults: new { controller = "UserSocialNetworks" }
            );

            config.Routes.MapHttpRoute(
                name: "ValidateRoutingNumber",
                routeTemplate: "api/routingnumber/validate",
                defaults: new { controller = "RoutingNumber", action = "ValidateRoutingNumber" }
            );

            config.Routes.MapHttpRoute(
               name: "BatchTransactions",
               routeTemplate: "api/batches/{batchId}/transactions",
               defaults: new { controller = "BatchTransactions" }
           );

            config.Routes.MapHttpRoute(
               name: "BatchServices",
               routeTemplate: "api/batch/batch_transactions",
               defaults: new { controller = "Batch", action = "BatchTransactions" }
            );
            config.Routes.MapHttpRoute(
               name: "SignUpKeySMSListner",
               routeTemplate: "api/MobileNumberSignUpKeySMSListenerController",
               defaults: new { controller = "MobileNumberSignUpKeySMSListenerController", action = "Post" },
               constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );
            config.Routes.MapHttpRoute(
               name: "PaystreamMessagesPost",
               routeTemplate: "api/paystreammessages",
               defaults: new { controller = "PaystreamMessages", action = "Post" },
               constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );
            config.Routes.MapHttpRoute(
               name: "Donate",
               routeTemplate: "api/paystreammessages/donate",
               defaults: new { controller = "PaystreamMessages", action = "Donate" }
           );

            config.Routes.MapHttpRoute(
                name: "AcceptPledge",
                routeTemplate: "api/paystreammessages/accept_pledge",
                defaults: new { controller = "PaystreamMessages", action = "AcceptPledge" }
            );
            config.Routes.MapHttpRoute(
                name: "CancelPayment",
                routeTemplate: "api/paystreammessages/{id}/cancel_payment",
                defaults: new { controller = "PaystreamMessages", action = "CancelPayment" }
            );
            config.Routes.MapHttpRoute(
                name: "RefundPayment",
                routeTemplate: "api/paystreammessages/{id}/refund_payment",
                defaults: new { controller = "PaystreamMessages", action = "RefundPayment" }
            );
            config.Routes.MapHttpRoute(
                name: "AcceptPaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/accept_request",
                defaults: new { controller = "PaystreamMessages", action = "AcceptPaymentRequest" }
            );
            config.Routes.MapHttpRoute(
                name: "RejectPaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/reject_request",
                defaults: new { controller = "PaystreamMessages", action = "RejectPaymentRequest" }
            );
            config.Routes.MapHttpRoute(
                name: "CancelPaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/cancel_request",
                defaults: new { controller = "PaystreamMessages", action = "CancelRequest" }
            );

            config.Routes.MapHttpRoute(
                name: "IgnorePaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/ignore_request",
                defaults: new { controller = "PaystreamMessages", action = "IgnorePaymentRequest" }
            );
            config.Routes.MapHttpRoute(
                name: "UpdateMessagesSeen",
                routeTemplate: "api/paystreammessages/{userId}/update_messages_seen",
                defaults: new { controller = "PaystreamMessages", action = "UpdateMessagesSeen" }
            );
            config.Routes.MapHttpRoute(
                name: "MultipleURIRequest",
                routeTemplate: "api/paystreammessages/multiple_uris",
                defaults: new { controller = "PaystreamMessages", action = "DetermineRecipient" }
            );
            // hey... this is a comment.
            // /api/Users/{0}/attributes
            config.Routes.MapHttpRoute(
                name: "UserAttribute",
                routeTemplate: "api/users/{userId}/attributes/{id}",
                defaults: new { controller = "UserAttributes", id = RouteParameter.Optional }
            );
            //api/user/{id}/personalize_user
            config.Routes.MapHttpRoute(
                name: "PersonalizeUser",
                routeTemplate: "api/users/{id}/personalize_user",
                defaults: new { controller = "Users", action = "PersonalizeUser" }
            );
            config.Routes.MapHttpRoute(
                name: "ValidateSecurityQuestion",
                routeTemplate: "api/users/{id}/validate_security_question",
                defaults: new { controller = "SecurityQuestions", action = "ValidateSecurityQuestion" }
            );

            config.Routes.MapHttpRoute(
                name: "SetupSecurityPin",
                routeTemplate: "api/users/{id}/setup_securitypin",
                defaults: new { controller = "Users", action = "SetupSecurityPin" }
            );


            config.Routes.MapHttpRoute(
                name: "ChangeSecurityPin",
                routeTemplate: "api/users/{id}/change_securitypin",
                defaults: new { controller = "Users", action = "ChangeSecurityPin" }
            );
            config.Routes.MapHttpRoute(
                name: "RefreshHomePageInformation",
                routeTemplate: "api/users/{id}/refresh_homepage",
                defaults: new { controller = "Users", action = "RefreshHomepageInformation" }
            );
            config.Routes.MapHttpRoute(
                name: "Join",
                routeTemplate: "api/users",
                defaults: new { controller = "Users", action = "Post" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );
            config.Routes.MapHttpRoute(
                 name: "GetUser",
                 routeTemplate: "api/users/{id}",
                 defaults: new { controller = "Users", action = "Get" },
                 constraints: new { httpMethod = new HttpMethodConstraint("GET") }
             );
            config.Routes.MapHttpRoute(
                name: "SignInWithFacebook",
                routeTemplate: "api/users/signin_withfacebook",
                defaults: new { controller = "Users", action = "SignInWithFacebook" }
            );

            config.Routes.MapHttpRoute(
                name: "LinkFacebook",
                routeTemplate: "api/users/{id}/link_facebook",
                defaults: new { controller = "Users", action = "LinkFacebook" }
            );

            config.Routes.MapHttpRoute(
                name: "ValidateUser",
                routeTemplate: "api/users/validate_user",
                defaults: new { controller = "Users", action = "ValidateUser" }
            );
            config.Routes.MapHttpRoute(
                name: "VerifyMobilePayPoint",
                routeTemplate: "api/users/{userId}/PayPoints/{id}/verify_mobile_paypoint",
                defaults: new { controller = "UserPayPoint", action = "VerifyMobilePayPoint" }
            );
            config.Routes.MapHttpRoute(
                name: "VerifyPayPoint",
                routeTemplate: "api/users/verify_paypoint",
                defaults: new { controller = "Users", action = "VerifyPayPoint" }
            );
            config.Routes.MapHttpRoute(
                name: "PayPoints",
                routeTemplate: "api/users/{userId}/PayPoints",
                defaults: new { controller = "UserPayPoint", action = "Post" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );
            config.Routes.MapHttpRoute(
                name: "GetMatchingMECodesWithSearchTerm",
                routeTemplate: "api/users/searchbymecode/{searchTerm}",
                defaults: new { controller = "Users", action = "GetMatchingMECodesWithSearchTerm", type = "" }
            );

            config.Routes.MapHttpRoute(
                name: "ResendMobileVerificationCode",
                routeTemplate: "api/users/{userId}/PayPoints/resend_mobile_verification_code",
                defaults: new { controller = "UserPayPoint", action = "ResendVerificationCode" }
            );
            config.Routes.MapHttpRoute(
                name: "ResendEmailVerificationLink",
                routeTemplate: "api/users/{userId}/PayPoints/resend_email_verification_link",
                defaults: new { controller = "UserPayPoint", action = "ResendEmailVerificationLink" }
            );
            config.Routes.MapHttpRoute(
                name: "UserPayPoints",
                routeTemplate: "api/users/{userId}/PayPoints",
                defaults: new { controller = "UserPayPoint" }
            );
            config.Routes.MapHttpRoute(
    name: "UserPayPointsWithType",
    routeTemplate: "api/users/{userId}/PayPoints/{id}/{type}",
    defaults: new { controller = "UserPayPoint", id = RouteParameter.Optional, type = RouteParameter.Optional }
);
            config.Routes.MapHttpRoute(
                name: "UserMECodes",
                routeTemplate: "api/users/{userId}/mecodes/{id}",
                defaults: new { controller = "UserMeCodes", id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "UploadMemberImage",
                routeTemplate: "api/users/{id}/upload_member_image",
                defaults: new { controller = "Users", action = "UploadMemberImage" }
            );
            config.Routes.MapHttpRoute(
                name: "ChangePassword",
                routeTemplate: "api/users/{id}/change_password",
                defaults: new { controller = "Users", action = "ChangePassword" }
            );
            config.Routes.MapHttpRoute(
                name: "ForgotPassword",
                routeTemplate: "api/users/forgot_password",
                defaults: new { controller = "Users", action = "ForgotPassword" }
            );
            config.Routes.MapHttpRoute(
                name: "ValidatePasswordReset",
                routeTemplate: "api/users/validate_passwordreset",
                defaults: new { controller = "Users", action = "ValidatePasswordResetAttempt" }
            );
            config.Routes.MapHttpRoute(
                name: "ResetPasswordEmail",
                routeTemplate: "api/users/reset_password",
                defaults: new { controller = "Users", action = "ResetPassword" }
            );

            config.Routes.MapHttpRoute(
                name: "SendEmail",
                routeTemplate: "api/users/{id}/send_email",
                defaults: new { controller = "Users", action = "SendEmail" }
            );

            config.Routes.MapHttpRoute(
            name: "RegisterPushNotifications",
            routeTemplate: "api/users/{id}/registerpushnotifications",
            defaults: new { controller = "Users", action = "RegisterForPushNotifications" }
            );

            config.Routes.MapHttpRoute(
                name: "PaymentAccounts",
                routeTemplate: "api/users/{userId}/PaymentAccounts",
                defaults: new { controller = "UserPaymentAccounts", action = "Post" },
                   constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            
            );
            config.Routes.MapHttpRoute(
                name: "DeletePaymentAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}",
                defaults: new { controller = "UserPaymentAccounts", action = "Delete" },
                   constraints: new { httpMethod = new HttpMethodConstraint("DELETE") }

            );

            config.Routes.MapHttpRoute(
                name: "AddUserPaymentAccounts",
                routeTemplate: "api/users/{userId}/PaymentAccounts/add_account",
                defaults: new { controller = "UserPaymentAccounts", action = "AddAccount" }
            );

            config.Routes.MapHttpRoute(
name: "UploadCheckImage",
routeTemplate: "api/users/{id}/PaymentAccounts/upload_check_image",
defaults: new { controller = "UserPaymentAccounts", action = "UploadCheckImage" }
);
            config.Routes.MapHttpRoute(
                name: "SetPreferredSendAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/set_preferred_send_account",
                defaults: new { controller = "UserPaymentAccounts", action = "SetPreferredSendAccount" }
            );

            config.Routes.MapHttpRoute(
                name: "SetPreferredReceiveAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/set_preferred_receive_account",
                defaults: new { controller = "UserPaymentAccounts", action = "SetPreferredReceiveAccount" }
            );

            config.Routes.MapHttpRoute(
                name: "VerifyACHAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}/verify_account",
                defaults: new { controller = "UserPaymentAccounts", action = "VerifyAccount" }
            );
            config.Routes.MapHttpRoute(
                name: "AddPaymentAccountVerification",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}/add_verification",
                defaults: new { controller = "UserPaymentAccounts", action = "AddVerification" }
            );

            config.Routes.MapHttpRoute(
                name: "UserConfigurations",
                routeTemplate: "api/users/{userId}/Configurations/{id}",
                defaults: new { controller = "UserConfigurations", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserPaymentAccounts",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}",
                defaults: new { controller = "UserPaymentAccounts", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserPayStreamMessages",
                routeTemplate: "api/users/{userId}/PayStreamMessages",
                defaults: new { controller = "UserPayStreamMessages" }
            );
            config.Routes.MapHttpRoute(
                name: "UserPayStreamMessage",
                routeTemplate: "api/users/{userId}/PayStreamMessages/{id}",
                defaults: new { controller = "UserPayStreamMessages" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
