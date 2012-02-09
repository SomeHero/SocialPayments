using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

// Add using statements to access AWS SDK for .NET services. 
// Both the Service and its Model namespace need to be added 
// in order to gain access to a service. For example, to access
// the EC2 service, add:
// using Amazon.EC2;
// using Amazon.EC2.Model;

namespace SocialPayments.Workflows.PaymentWorkflow
{
    class Program
    {
        public static void Main(string[] args)
        {

            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();
            client.ConfirmSubscription(new ConfirmSubscriptionRequest()
            {
                a
            });
            client.Publish(new PublishRequest() {
                Message = "New Payment Receivied",
                TopicArn = "arn:aws:sns:us-east-1:102476399870:SocialPaymentNotifications",
                Subject = "Payment from 804-387-9693"
            });

           
            Console.Read();
        }
    }
}