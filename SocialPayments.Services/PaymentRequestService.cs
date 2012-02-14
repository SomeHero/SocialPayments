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
using SocialPayments.Services.DataContracts.PaymentRequest;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PaymentRequestService : IPaymentRequestService
    {
        private DomainServices.SecurityService securityService = new DomainServices.SecurityService();
        DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();
        DomainServices.UserService userService = new DomainServices.UserService();
        private Context _ctx = new Context();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DataContracts.PaymentRequest.PaymentRequestResponse AddPaymentRequest(DataContracts.PaymentRequest.PaymentRequestRequest request)
        {
            Domain.PaymentRequest newPaymentRequest;

            var application = applicationService.GetApplication(Guid.Parse("bda11d91-7ade-4da1-855d-24adfe39d174"));

            logger.Log(LogLevel.Info, String.Format("Getting Application."));

            if (application == null)
            {
                logger.Log(LogLevel.Info, String.Format("No application"));

                return new PaymentRequestResponse()
                {
                    Success = false,
                    Message = "Invalid Api Key."
                };
            }

            logger.Log(LogLevel.Info, String.Format("Getting Requestor."));

            var payer = userService.GetUser(u => u.UserId.Equals(new Guid(request.UserId)));

            if (payer == null)
            {
                logger.Log(LogLevel.Info, String.Format("No payer found."));

                return new PaymentRequestResponse()
                {
                    Success = false,
                    Message = "Payment Request from an unknown User."
                };
            }
            string securityPin;

            logger.Log(LogLevel.Info, String.Format("Validating security pin."));

            try
            {
                securityPin = securityService.Decrypt(payer.SecurityPin);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Exception decrypting security pin {0}. Exception {1}.", request.SecurityPin, ex.Message));

                return new PaymentRequestResponse()
                {
                    Success = false,
                    Message = "Incorrect security pin.  Try again."
                };
            }

            if (securityPin != request.SecurityPin)
            {
                logger.Log(LogLevel.Warn, String.Format("Security Pin was incorrect"));

                //if 3 incorrect swipes within the last 15 minutes
                //send back Lockout message
                //else
                //send SecurityPinIncorrect message back to phone
                return new PaymentRequestResponse()
                {
                    Success = false,
                    Message = "Incorrect Security Pin.  Try again."
                };
            }

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