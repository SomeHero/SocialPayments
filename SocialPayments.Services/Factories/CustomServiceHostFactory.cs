using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using System.ServiceModel;
using SocialPayments.Services.Behaviors;
using SocialPayments.Services.Inspectors;

namespace SocialPayments.Services
{
    public class CustomServiceHostFactory: WebServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var host = base.CreateServiceHost(serviceType, baseAddresses);
            host.Description.Behaviors.Add(new MessageLoggingBehavior());

            return host;
        }
    }
}