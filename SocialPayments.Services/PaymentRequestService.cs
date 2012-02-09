using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using SocialPayments.DataLayer;
using System.ServiceModel.Activation;
using NLog;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PaymentRequestService : IPaymentRequestService
    {
        private DomainServices.SecurityService securityService = new DomainServices.SecurityService();
        private Context _ctx = new Context();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DataContracts.PaymentRequest.PaymentRequestResponse AddPaymentRequest(DataContracts.PaymentRequest.PaymentRequestRequest request)
        {
            Domain.PaymentRequest newPaymentRequest;

            try
            {
                newPaymentRequest = _ctx.PaymentRequests.Add(new SocialPayments.Domain.PaymentRequest()
                {
                    ApiKey = new Guid(request.ApiKey),
                    Amount = request.Amount,
                    Comments = request.Comments,
                    CreateDate = System.DateTime.Now,
                    PaymentRequestId = Guid.NewGuid(),
                    PaymentRequestStatus = Domain.PaymentRequestStatus.Submitted,
                    RecipientUri = request.RecipientUri,
                    RequestorId = new Guid(request.UserId),
                    UpdateDate = null
                });

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Unhandled exception submitting payment request. Exception {0}.", ex.Message));

                return new DataContracts.PaymentRequest.PaymentRequestResponse()
                {
                    Success = false,
                    Message = "Failed to submit payment request.  Please try again."
                };
            }

            logger.Log(LogLevel.Info, String.Format("Payment request successfully submitted."));

            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

            try
            {
                logger.Log(LogLevel.Info, String.Format("Attempting to send message to Amazon"));

                client.Publish(new PublishRequest()
                {
                    Message = newPaymentRequest.PaymentRequestId.ToString(),
                    TopicArn = "arn:aws:sns:us-east-1:102476399870:PaymentRequests",
                    Subject = "New Payment Request Receivied"
                });
                logger.Log(LogLevel.Info, String.Format("Message sent to Amazon SNS"));

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Exception sending message to Amazon SNS. Exception: {0}.", ex.Message));
            }
            
            DataContracts.Payment.PaymentResponse results;

            return new DataContracts.PaymentRequest.PaymentRequestResponse()
            {
                Success = true,
                Message = String.Format("Your Payment Request to {0} for {1:C} was successfully submitted.  Thanks for using PaidThx", request.RecipientUri, request.Amount)
            };
        }
    }
}