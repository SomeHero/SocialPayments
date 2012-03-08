using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Data.Entity;
using SocialPayments.DataLayer;

namespace SocialPayments.Services
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
           // Database.SetInitializer(new MyInitializer());

            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Edit the base address of Service1 by replacing the "Service1" string below
            RouteTable.Routes.Add(new ServiceRoute("PaymentService", new WebServiceHostFactory(), typeof(PaymentService)));
            RouteTable.Routes.Add(new ServiceRoute("PaymentRequestService", new WebServiceHostFactory(), typeof(PaymentRequestService))); 
            RouteTable.Routes.Add(new ServiceRoute("PaymentAccountService", new WebServiceHostFactory(), typeof(PaymentAccountService)));
            RouteTable.Routes.Add(new ServiceRoute("PaymentProcessingService", new WebServiceHostFactory(), typeof(PaymentProcessingService)));
            RouteTable.Routes.Add(new ServiceRoute("UserService", new WebServiceHostFactory(), typeof(UserService)));
            RouteTable.Routes.Add(new ServiceRoute("CalendarService", new WebServiceHostFactory(), typeof(CalendarService)));
            RouteTable.Routes.Add(new ServiceRoute("ApplicationService", new WebServiceHostFactory(), typeof(ApplicationService)));
            RouteTable.Routes.Add(new ServiceRoute("EmailService", new WebServiceHostFactory(), typeof(EmailService)));
            RouteTable.Routes.Add(new ServiceRoute("SMSService", new WebServiceHostFactory(), typeof(SMSService)));

        }
    }
}
