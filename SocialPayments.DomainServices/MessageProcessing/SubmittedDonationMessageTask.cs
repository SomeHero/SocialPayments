using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Configuration;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class SubmittedDonationMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();
        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSetURL"];
        private string _mobileWebSiteEngageURl = ConfigurationManager.AppSettings["MobileWebSetURL"];

        public void Execute(Guid messageId)
        {

        }
    }
}
