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
            //api/user/{id}/personalize_user
            routes.MapHttpRoute(
                name: "PersonalizeUser",
                routeTemplate: "api/users/{id}/personalize_user",
                defaults: new { controller = "Users", action = "PersonalizeUser" }
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
                name: "ValidateUser",
                routeTemplate: "api/users/validate_user",
                defaults: new { controller = "Users", action = "ValidateUser" }
            );

            routes.MapHttpRoute(
                name: "UserMECodes",
                routeTemplate: "api/users/{userId}/mecodes/{id}",
                defaults: new { controller = "UserMeCodes", id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "ChangePassword",
                routeTemplate: "api/users/{id}/change_password",
                defaults: new { controller = "Users", action = "ChangePassword" }
            );

            routes.MapHttpRoute(
            name: "RegisterPushNotifications",
            routeTemplate: "api/users/{id}/registerpushnotifications",
            defaults: new { controller = "Users", action = "RegisterForPushNotifications" }
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