﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.Interfaces
{
    public interface IAmazonNotificationService
    {
        void PushSNSNotification(string topicARN, string subject, string message);
    }
}
