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

namespace SocialPayments.RestServices.External
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
                name: "SearchMessagesByDate",
                routeTemplate: "api/messages/date={date}",
                defaults: new { controller = "Messages", action = "GetMessagesByDate", messageDate = "{date}" }
            );

            routes.MapHttpRoute(
                name: "UserAccounts",
                routeTemplate: "api/users/{id}/accounts/{accountId}",
                defaults: new { controller = "UserAccounts", accountId = UrlParameter.Optional });

            routes.MapHttpRoute(
                name: "UserAttributes",
                routeTemplate: "api/users/{id}/attributes/{attributeId}",
                defaults: new { controller = "UserAttributes", attributeId = UrlParameter.Optional });

            routes.MapHttpRoute(
                name: "UserMessages",
                routeTemplate: "api/users/{id}/messages/{messageId}",
                defaults: new { controller = "UserMessages", messageId = UrlParameter.Optional });

            routes.MapHttpRoute(
                name: "UserTransactions",
                routeTemplate: "api/users/{id}/transactions/{transactionId}",
                defaults: new { controller = "UserTransactions", transactionId = UrlParameter.Optional });

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