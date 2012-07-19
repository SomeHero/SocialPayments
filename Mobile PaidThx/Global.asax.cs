using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using SocialPayments.DataLayer;

namespace Mobile_PaidThx
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "ResetPassword", // Route name
                "reset_password/{id}", // URL with parameters
                new { controller = "Account", action = "ResetPassword" } // Parameter defaults
            );
            routes.MapRoute(
                "RedirectShortner", // Route name
                "i/{id}", // URL with parameters
                new { controller = "Home", action = "ClaimPayment" } // Parameter defaults
            );
            routes.MapRoute(
                "Help", // Route name
                "Help", // URL with parameters
                new { controller = "Help", action = "Index" } // Parameter defaults
            );
            routes.MapRoute(
                "Privacy", // Route name
                "Privacy", // URL with parameters
                new { controller = "Privacy", action = "Index" } // Parameter defaults
            );
            routes.MapRoute(
                   "PaymentAccount",
                   "PaymentAccount/{action}/{id}",
                   new { controller = "PaymentAccount", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "SignInWithFacebook", // Route name
                "SignInWithFacebook", // URL with parameters
                new { controller = "Account", action = "SignInWithFacebook" } // Parameter defaults
            );
            routes.MapRoute(
                "RegisterWithFacebook", // Route name
                "RegisterWithFacebook", // URL with parameters
                new { controller = "Register", action = "RegisterWithFacebook" } // Parameter defaults
            );
            routes.MapRoute(
                "Register", // Route name
                "Register", // URL with parameters
                new { controller = "Register", action = "Index" } // Parameter defaults
            );
            routes.MapRoute(
                "SignIn", // Route name
                "SignIn", // URL with parameters
                new { controller = "SignIn", action = "Index" } // Parameter defaults
            );
            routes.MapRoute(
                "Paystream", // Route name
                "Paystream/{action}", // URL with parameters
                new { controller = "Paystream", action = "Index" } // Parameter defaults
            );
            routes.MapRoute(
                "Profile", // Route name
                "Profile/{action}/{id}", // URL with parameters
                new { controller = "Profile", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Receive", // Route name
                "{messageId}", // URL with parameters
                new { controller = "Home", action = "Index" } // Parameter defaults
            );
            
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}", // URL with parameters
                new { controller = "Account", action = "SignIn" } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            Database.SetInitializer(new MyInitializer());
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}