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

namespace SocialPayments.RestServices.Internal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHttpRoute(
                name: "ValidateUser",
                routeTemplate: "api/users/validate_user",
                defaults: new { controller = "Users", action = "ValidateUser" }
            );

            routes.MapHttpRoute(
                name: "UserPaymentAccounts",
                routeTemplate: "api/{userId}/PaymentAccounts/{id}",
                defaults: new { controller = "UserPaymentAccounts", id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "UserPayStreamMessages",
                routeTemplate: "api/{userId}/PayStreamMessages/{id}",
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
            Database.SetInitializer(new MyInitializer());
            
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            BundleTable.Bundles.RegisterTemplateBundles();
        }
    }
}