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
            filters.Add(new System.Web.Mvc.AuthorizeAttribute());
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
            WebApiConfig.Register(config);
        }
    }
}