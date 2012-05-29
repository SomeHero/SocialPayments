using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DomainServices.Interfaces;
using Amazon.SimpleNotificationService;
using NLog;
using Amazon.SimpleNotificationService.Model;

namespace SocialPayments.DomainServices
{
    public class AmazonNotificationService: IAmazonNotificationService
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public void PushSNSNotification(string topicARN, string subject, string message)
        {
            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

            try
            {
                _logger.Log(LogLevel.Info, String.Format("Pushing Message {0} to Amazon SNS", message));

                client.Publish(new PublishRequest()
                {
                    Message = message,
                    TopicArn = topicARN,
                    Subject = subject
                });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Exception Pusing Message {0} to Amazon SNS. {1}", message, ex.Message));
            }
        }
    }
}
