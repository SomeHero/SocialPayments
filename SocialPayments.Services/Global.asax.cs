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
            RouteTable.Routes.Add(new ServiceRoute("PaymentService", new CustomServiceHostFactory(), typeof(PaymentService)));
            RouteTable.Routes.Add(new ServiceRoute("PaymentRequestService", new CustomServiceHostFactory(), typeof(PaymentRequestService))); 
            RouteTable.Routes.Add(new ServiceRoute("PaymentAccountService", new CustomServiceHostFactory(), typeof(PaymentAccountService)));
            RouteTable.Routes.Add(new ServiceRoute("PaymentProcessingService", new CustomServiceHostFactory(), typeof(PaymentProcessingService)));
            RouteTable.Routes.Add(new ServiceRoute("UserService", new CustomServiceHostFactory(), typeof(UserService)));
            RouteTable.Routes.Add(new ServiceRoute("CalendarService", new CustomServiceHostFactory(), typeof(CalendarService)));
            RouteTable.Routes.Add(new ServiceRoute("ApplicationService", new CustomServiceHostFactory(), typeof(ApplicationService)));
            RouteTable.Routes.Add(new ServiceRoute("EmailService", new CustomServiceHostFactory(), typeof(EmailService)));
            RouteTable.Routes.Add(new ServiceRoute("SMSService", new CustomServiceHostFactory(), typeof(SMSService)));

        }
    }
}
