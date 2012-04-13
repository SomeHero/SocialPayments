using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using System.ServiceModel.Activation;
using System.ServiceModel;
using SocialPayments.Services.DataContracts;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using NLog;
using SocialPayments.Services.DataContracts.Payment;
using SocialPayments.DataLayer;
using System.Configuration;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    // NOTE: If the service is renamed, remember to update the global.asax.cs file
    public class PaymentService : IPaymentService
    {
        DomainServices.PaymentService paymentService = new DomainServices.PaymentService();
        DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();
        DomainServices.UserService userService = new DomainServices.UserService();
        private DomainServices.SecurityService securityService = new DomainServices.SecurityService();
        private Context _ctx = new Context();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<DataContracts.Payment.PaymentResponse> GetPayments()
        {
            logger.Log(LogLevel.Info, String.Format("Getting Payments"));

            var payments = paymentService.GetPayments(p => p);

            return payments.Select(p => new DataContracts.Payment.PaymentResponse()
            {
                Amount = p.PaymentAmount,
                Comment = p.Comments,
                FromAccount = new DataContracts.PaymentAccount.PaymentAccountReponse()
                {

                },
                FromMobileNumber = p.FromMobileNumber,
                ToMobileNumber = p.ToMobileNumber,
                PaymentId = p.Id.ToString(),
                UserId = p.FromAccount.User.UserId.ToString()
            }).ToList();
        }
        public DataContracts.Payment.PaymentResponse GetPayment(string id)
        {
            logger.Log(LogLevel.Info, String.Format("Getting Payment {0}", id));

            DomainServices.PaymentService paymentService = new DomainServices.PaymentService();

            Guid paymentId;

            Guid.TryParse(id, out paymentId);

            var payment = _ctx.Payments
                .Include("Application")
                .Include("FromAccount")
                .Include("FromAccount.User")
                .Include("ToAccount")
                .Include("Transactions")
                .Include("Transactions.FromAccount")
                .FirstOrDefault(p => p.Id == paymentId);

            DataContracts.Transaction.TransactionResponse senderTransaction = null;
            DataContracts.Transaction.TransactionResponse recipientTransaction = null;

            foreach (var transaction in payment.Transactions)
            {
                logger.Log(LogLevel.Info, transaction.FromAccount.AccountNumber);
                
                var accountInfo = securityService.Decrypt(transaction.FromAccount.AccountNumber);
                var accountType = transaction.FromAccount.AccountType.ToString();

                logger.Log(LogLevel.Info, accountInfo);
                if(accountInfo.Length >= 4)
                    accountInfo = accountInfo.Substring(accountInfo.Length - 4, 4);

                if (transaction.Type == Domain.TransactionType.Withdrawal)
                {
                    senderTransaction = new DataContracts.Transaction.TransactionResponse()
                    {
                        ACHTransactionId = transaction.ACHTransactionId,
                        Amount = transaction.Amount,
                        CreateDate = transaction.CreateDate.ToString("MM/dd/yyyy hh:mm tt"),
                        FromAccount = new DataContracts.PaymentAccount.PaymentAccountReponse()
                        {
                            Id = transaction.FromAccount.Id.ToString(),
                            AccountInformation = accountType + " " + "****" + accountInfo
                        },
                        LastUpdatedDate = transaction.LastUpdatedDate,
                        PaymentChannel = transaction.PaymentChannelType.ToString(),
                        PaymentId = transaction.PaymentId,
                        StandardEntryClass = transaction.StandardEntryClass.ToString(),
                        TransactionBatchId = transaction.TransactionBatchId.ToString(),
                        TransactionCategory = "Withdrawal",
                        TransactionId = transaction.Id,
                        TransactionStatus = transaction.Status.ToString(),
                        TransationSentDate = transaction.SentDate
                    };

                }
                else
                {
                    recipientTransaction = new DataContracts.Transaction.TransactionResponse()
                    {
                        ACHTransactionId = transaction.ACHTransactionId,
                        Amount = transaction.Amount,
                        CreateDate = transaction.CreateDate.ToString("MM/dd/yyyy hh:mm tt"),
                        FromAccount = new DataContracts.PaymentAccount.PaymentAccountReponse()
                        {
                            Id = transaction.FromAccount.Id.ToString(),
                            AccountInformation = accountType + " " + "****" + accountInfo
                        },
                        LastUpdatedDate = transaction.LastUpdatedDate,
                        PaymentChannel = transaction.PaymentChannelType.ToString(),
                        PaymentId = transaction.PaymentId,
                        StandardEntryClass = transaction.StandardEntryClass.ToString(),
                        TransactionBatchId = transaction.TransactionBatchId.ToString(),
                        TransactionCategory = "Deposit",
                        TransactionId = transaction.Id,
                        TransactionStatus = transaction.Status.ToString(),
                        TransationSentDate = transaction.SentDate
                    };
                }
            }

            return new DataContracts.Payment.PaymentResponse()
            {
                PaymentId = payment.Id.ToString(),
                FromAccount = new DataContracts.PaymentAccount.PaymentAccountReponse()
                {
                     
                },
                FromMobileNumber = payment.FromMobileNumber,
                Amount = payment.PaymentAmount,
                ToMobileNumber = payment.ToMobileNumber,
                Comment = payment.Comments,
                UserId = payment.FromAccount.User.UserId.ToString(),
                SenderTransaction = senderTransaction,
                RecipientTransaction = recipientTransaction,
                CreateDate = payment.CreateDate,
                PaymentSubmittedDate = payment.PaymentDate,
                PaymentStatus = payment.PaymentStatus.ToString()
            };
        }
        public DataContracts.Payment.CancelPaymentResponse CancelPayment(DataContracts.Payment.CancelPaymentRequest request) {
            var payment = _ctx.Payments.FirstOrDefault(p => p.Id == new Guid(request.PaymentId));

            if (payment == null)
            {
                return new CancelPaymentResponse()
                {
                    Success = false,
                    Message = String.Format("Unable to cancel payment {0}. Payment not found.", request.PaymentId)
                };
            }

            if (payment.PaymentStatus != Domain.PaymentStatus.Submitted || payment.PaymentStatus != Domain.PaymentStatus.ReturnedNSF)
            {
                return new CancelPaymentResponse()
                {
                    Success = false,
                    Message = String.Format("Unable to cancel payment {0}. Payment not a valid status.", request.PaymentId)
                };
            }
            payment.PaymentStatus = Domain.PaymentStatus.Cancelled;

            foreach (var transaction in payment.Transactions)
            {
                transaction.Status = Domain.TransactionStatus.Cancelled;
            }

            _ctx.SaveChanges();

            return new CancelPaymentResponse()
            {
                Success = true
            };
        }
        public DataContracts.Payment.PaymentResponse AddPayment(DataContracts.Payment.PaymentRequest paymentRequest)
        {
            logger.Log(LogLevel.Info, String.Format("Payment Service received request to add payment."));

            var application = applicationService.GetApplication(Guid.Parse("bda11d91-7ade-4da1-855d-24adfe39d174"));

            logger.Log(LogLevel.Info, String.Format("Getting Application."));

            if (application == null)
            {
                logger.Log(LogLevel.Info, String.Format("No application"));

                return new PaymentResponse()
                {
                    ResponseStatus = "Failed",
                    Success = false,
                    Message = "Invalid Api Key."
                };
            }

            logger.Log(LogLevel.Info, String.Format("Getting Payer."));

            var payer = userService.GetUser(u => u.UserId.Equals(new Guid(paymentRequest.UserId)));

            if (payer == null)
            {
                logger.Log(LogLevel.Info, String.Format("No payer found."));

                return new PaymentResponse()
                {
                    ResponseStatus = "Failed",
                    Success = false,
                    Message = "Payment Request from an Unknown User."
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
                logger.Log(LogLevel.Error, String.Format("Exception decrypting security pin {0}. Exception {1}.", paymentRequest.SecurityPin, ex.Message));

                return new PaymentResponse()
                {
                    ResponseStatus = "Failed",
                    Success = false,
                    Message = "Security Pin Failed."
                };
            }

            if (securityPin != paymentRequest.SecurityPin)
            {
                logger.Log(LogLevel.Warn, String.Format("Security Pin was incorrect"));

                //if 3 incorrect swipes within the last 15 minutes
                //send back Lockout message
                //else
                //send SecurityPinIncorrect message back to phone
                return new PaymentResponse()
                {
                    ResponseStatus = "Failed",
                    Success = false,
                    Message = "Unable to Validate Payment Request."
                };
            }

            logger.Log(LogLevel.Info, String.Format("Getting From Account."));

            var fromAccount = payer.PaymentAccounts[0];
            if (fromAccount == null)
                logger.Log(LogLevel.Info, String.Format("From Account not found."));


            logger.Log(LogLevel.Info, String.Format("Adding Payment."));

            Domain.Payment payment;

            try
            {
                payment = _ctx.Payments.Add(new Domain.Payment()
                {
                    Id = Guid.NewGuid(),
                    ApiKey = application.ApiKey,
                    Comments = paymentRequest.Comment,
                    CreateDate = System.DateTime.Now,
                    FromAccountId = fromAccount.Id,
                    FromMobileNumber = paymentRequest.FromMobileNumber,
                    PaymentAmount = paymentRequest.Amount,
                    PaymentChannelType = Domain.PaymentChannelType.Single,
                    PaymentDate = System.DateTime.Now,
                    PaymentStatus = Domain.PaymentStatus.Submitted,
                    StandardEntryClass = Domain.StandardEntryClass.Web,
                    ToMobileNumber = paymentRequest.ToMobileNumber
                });

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, String.Format("Failed adding payment. Exception {0}", ex.ToString()));

                return new PaymentResponse()
                {
                    ResponseStatus = "Failed",
                    Success = false,
                    Message = "Exception occurred while processing the payment request.  Please try again."
                };
            }

            _ctx.SaveChanges();
            logger.Log(LogLevel.Info, String.Format("Payment successfully submitted."));

            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

            client.Publish(new PublishRequest()
            {
                Message = payment.Id.ToString(),
                TopicArn = ConfigurationManager.AppSettings["PaymentPostedTopicARN"],
                Subject = "New Payment Receivied"
            });
            logger.Log(LogLevel.Info, String.Format("Message sent to Amazon SNS"));

            DataContracts.Payment.PaymentResponse results;

            try
            {
                results = new DataContracts.Payment.PaymentResponse()
                {
                    ResponseStatus = "OK",
                    Success = true,
                    Message = string.Format("Your payment for {0} was sent to {1}.", payment.PaymentAmount, payment.ToMobileNumber),
                    Amount = payment.PaymentAmount,
                    Comment = payment.Comments,
                    FromAccount = new DataContracts.PaymentAccount.PaymentAccountReponse()
                    {

                    },
                    FromMobileNumber = payment.FromMobileNumber,
                    PaymentId = payment.Id.ToString(),
                    ToMobileNumber = payment.ToMobileNumber,
                    UserId = paymentRequest.UserId,
                };

                return results;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, string.Format("Exception occurred {0}.", ex.Message));

                return new PaymentResponse()
                {
                    ResponseStatus = "Failed",
                    Success = false,
                    Message = string.Format("Exception occurred.")
                };
            }


        }

        public void UpdatePayment(DataContracts.Payment.PaymentRequest paymentRequest)
        {
            logger.Log(LogLevel.Info, String.Format("Updating Payment"));

            var payment = paymentService.GetPayment(new Guid(paymentRequest.PaymentId));

            payment.PaymentAmount = paymentRequest.Amount;
            payment.Comments = paymentRequest.Comment;
            payment.FromMobileNumber = paymentRequest.FromMobileNumber;
            payment.ToMobileNumber = paymentRequest.ToMobileNumber;

            paymentService.UpdatePayment(payment);
        }

        public void DeletePayment(DataContracts.Payment.PaymentRequest paymentRequest)
        {
            logger.Log(LogLevel.Info, String.Format("Deleting Payment"));

            paymentService.DeletePayment(new Guid(paymentRequest.PaymentId));
        }
    }
}