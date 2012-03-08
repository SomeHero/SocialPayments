using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Configuration;

namespace SocialPayments.Services.Behaviors
{
    public class MessageLoggingBehaviorExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new MessageLoggingBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(MessageLoggingBehavior);
            }
        }
    }
}