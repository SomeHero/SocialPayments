using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using SocialPayments.DataLayer;
using NLog;

namespace SocialPayments.RestServices.Internal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapHttpRoute(
               name: "BatchTransactions",
               routeTemplate: "api/batches/{batchId}/transactions",
               defaults: new { controller = "BatchTransactions" }
           );

            routes.MapHttpRoute(
               name: "Donate",
               routeTemplate: "api/paystreammessages/donate",
               defaults: new { controller = "PaystreamMessages", action = "Donate" }
           );

            routes.MapHttpRoute(
                name: "AcceptPledge",
                routeTemplate: "api/paystreammessages/accept_pledge",
                defaults: new { controller = "PaystreamMessages", action = "AcceptPledge" }
            );
            routes.MapHttpRoute(
                name: "CancelPayment",
                routeTemplate: "api/paystreammessages/{id}/cancel_payment",
                defaults: new { controller = "PaystreamMessages", action = "CancelPayment" }
            );
            routes.MapHttpRoute(
                name: "RefundPayment",
                routeTemplate: "api/paystreammessages/{id}/refund_payment",
                defaults: new { controller = "PaystreamMessages", action = "RefundPayment" }
            );
            routes.MapHttpRoute(
                name: "AcceptPaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/accept_request",
                defaults: new { controller = "PaystreamMessages", action = "AcceptPaymentRequest" }
            );
            routes.MapHttpRoute(
                name: "RejectPaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/reject_request",
                defaults: new { controller = "PaystreamMessages", action = "RejectPaymentRequest" }
            );
            routes.MapHttpRoute(
                name: "CancelPaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/cancel_request",
                defaults: new { controller = "PaystreamMessages", action = "CancelRequest" }
            );
            routes.MapHttpRoute(
                name: "IgnorePaymentRequest",
                routeTemplate: "api/paystreammessages/{id}/ignore_request",
                defaults: new { controller = "PaystreamMessages", action = "IgnorePaymentRequest" }
            );
            routes.MapHttpRoute(
                name: "UpdateMessagesSeen",
                routeTemplate: "api/paystreammessages/update_messages_seen",
                defaults: new { controller = "PaystreamMessages", action = "UpdateMessagesSeen" }
            );
            routes.MapHttpRoute(
                name: "MultipleURIRequest",
                routeTemplate: "api/paystreammessages/multiple_uris",
                defaults: new { controller = "PaystreamMessages", action = "DetermineRecipient" }
            );

            // /api/Users/{0}/attributes
            routes.MapHttpRoute(
                name: "UserAttribute",
                routeTemplate: "api/users/{userId}/attributes/{id}",
                defaults: new { controller = "UserAttributes", id = RouteParameter.Optional }
            );
            //api/user/{id}/personalize_user
            routes.MapHttpRoute(
                name: "PersonalizeUser",
                routeTemplate: "api/users/{id}/personalize_user",
                defaults: new { controller = "Users", action = "PersonalizeUser" }
            );
            routes.MapHttpRoute(
                name: "ValidateSecurityQuestion",
                routeTemplate: "api/users/{id}/validate_security_question",
                defaults: new { controller = "SecurityQuestions", action = "ValidateSecurityQuestion" }
            );

            routes.MapHttpRoute(
                name: "SetupSecurityPin",
                routeTemplate: "api/users/{id}/setup_securitypin",
                defaults: new { controller = "Users", action = "SetupSecurityPin" }
            );


            routes.MapHttpRoute(
                name: "ChangeSecurityPin",
                routeTemplate: "api/users/{id}/change_securitypin",
                defaults: new { controller = "Users", action = "ChangeSecurityPin" }
            );

            routes.MapHttpRoute(
                name: "SignInWithFacebook",
                routeTemplate: "api/users/signin_withfacebook",
                defaults: new { controller = "Users", action = "SignInWithFacebook" }
            );

            routes.MapHttpRoute(
                name: "LinkFacebook",
                routeTemplate: "api/users/{id}/link_facebook",
                defaults: new { controller = "Users", action = "LinkFacebook" }
            );

            routes.MapHttpRoute(
                name: "ValidateUser",
                routeTemplate: "api/users/validate_user",
                defaults: new { controller = "Users", action = "ValidateUser" }
            );
            routes.MapHttpRoute(
                name: "VerifyMobilePayPoint",
                routeTemplate: "api/users/{userId}/PayPoints/{id}/verify_mobile_paypoint",
                defaults: new { controller = "UserPayPoint", action = "VerifyMobilePayPoint" }
            );
            routes.MapHttpRoute(
                name: "VerifyPayPoint",
                routeTemplate: "api/users/verify_paypoint",
                defaults: new { controller = "Users", action = "VerifyPayPoint" }
            );

            routes.MapHttpRoute(
                name: "RefreshHomePageInformation",
                routeTemplate: "api/users/{id}/refresh_homepage",
                defaults: new { controller = "Users", action = "RefreshHomepageInformation" }
            );

            routes.MapHttpRoute(
                name: "ResendMobileVerificationCode",
                routeTemplate: "api/users/{userId}/PayPoints/resend_mobile_verification_code",
                defaults: new { controller = "UserPayPoint", action = "ResendVerificationCode" }
            );
            routes.MapHttpRoute(
                name: "ResendEmailVerificationLink",
                routeTemplate: "api/users/{userId}/PayPoints/resend_email_verification_link",
                defaults: new { controller = "UserPayPoint", action = "ResendEmailVerificationLink" }
            );
            routes.MapHttpRoute(
                name: "UserPayPoints",
                routeTemplate: "api/users/{userId}/PayPoints/{id}",
                defaults: new { controller = "UserPayPoint", id = RouteParameter.Optional }
            );
            routes.MapHttpRoute(
    name: "UserPayPointsWithType",
    routeTemplate: "api/users/{userId}/PayPoints/{id}/{type}",
    defaults: new { controller = "UserPayPoint", id = RouteParameter.Optional, type = RouteParameter.Optional }
);
            routes.MapHttpRoute(
                name: "UserMECodes",
                routeTemplate: "api/users/{userId}/mecodes/{id}",
                defaults: new { controller = "UserMeCodes", id = RouteParameter.Optional }
            );
            routes.MapHttpRoute(
                name: "UploadMemberImage",
                routeTemplate: "api/users/{id}/upload_member_image",
                defaults: new { controller = "Users", action = "UploadMemberImage" }
            );
            routes.MapHttpRoute(
                name: "ChangePassword",
                routeTemplate: "api/users/{id}/change_password",
                defaults: new { controller = "Users", action = "ChangePassword" }
            );

            routes.MapHttpRoute(
                name: "ResetPasswordEmail",
                routeTemplate: "api/users/reset_password",
                defaults: new { controller = "Users", action = "ResetPassword" }
            );

            routes.MapHttpRoute(
                name: "SendEmail",
                routeTemplate: "api/users/{id}/send_email",
                defaults: new { controller = "Users", action = "SendEmail" }
            );

            routes.MapHttpRoute(
            name: "RegisterPushNotifications",
            routeTemplate: "api/users/{id}/registerpushnotifications",
            defaults: new { controller = "Users", action = "RegisterForPushNotifications" }
            );

            routes.MapHttpRoute(
                name: "AddUserPaymentAccounts",
                routeTemplate: "api/users/{userId}/PaymentAccounts/add_account",
                defaults: new { controller = "UserPaymentAccounts", action = "AddAccount" }
            );

            routes.MapHttpRoute(
name: "UploadCheckImage",
routeTemplate: "api/users/{id}/PaymentAccounts/upload_check_image",
defaults: new { controller = "UserPaymentAccounts", action = "UploadCheckImage" }
);
            routes.MapHttpRoute(
                name: "SetPreferredSendAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/set_preferred_send_account",
                defaults: new { controller = "UserPaymentAccounts", action = "SetPreferredSendAccount" }
            );

            routes.MapHttpRoute(
                name: "SetPreferredReceiveAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/set_preferred_receive_account",
                defaults: new { controller = "UserPaymentAccounts", action = "SetPreferredReceiveAccount" }
            );

            routes.MapHttpRoute(
                name: "VerifyACHAccount",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}/verify_account",
                defaults: new { controller = "UserPaymentAccounts", action = "VerifyAccount" }
            );            
            routes.MapHttpRoute(
                name: "AddPaymentAccountVerification",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}/add_verification",
                defaults: new { controller = "UserPaymentAccounts", action = "AddVerification" }
            );

            routes.MapHttpRoute(
                name: "UserConfigurations",
                routeTemplate: "api/users/{userId}/Configurations/{id}",
                defaults: new { controller = "UserConfigurations", id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "UserPaymentAccounts",
                routeTemplate: "api/users/{userId}/PaymentAccounts/{id}",
                defaults: new { controller = "UserPaymentAccounts", id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "UserPayStreamMessages",
                routeTemplate: "api/users/{userId}/PayStreamMessages/{id}",
                defaults: new { controller = "UserPayStreamMessages", id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            var config = GlobalConfiguration.Configuration;
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            _logger.Log(LogLevel.Info, String.Format("Starting application {0}", "API Internal"));

            try
            {
                Database.SetInitializer(new MyInitializer());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Failed to initialize database. {0}", ex.Message));

                throw ex;
            }

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            BundleTable.Bundles.RegisterTemplateBundles();
        }
    }
}