using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using SocialPayments.Domain;

namespace SocialPayments.Services
{
    public class PaymentProcessingService: IPaymentProcessingService
    {
        DomainServices.PaymentService paymentService = new DomainServices.PaymentService();
        DomainServices.UserService userService = new DomainServices.UserService();
        EmailService emailService = new EmailService();
        SMSService smsService = new SMSService();
                    
        public void ProcessPayment(DataContracts.SubmittedPaymentRequest submittedPaymentRequest)
        {

            var payment = paymentService.GetPayment(new Guid(submittedPaymentRequest.Message));

            switch (payment.PaymentStatus)
            {
                case PaymentStatus.Submitted:
                    string fromAddress = "jrhodes2705@gmail.com";

                    //Validate Payment
                    //Attempt to assign payment to Payee
                    var payee = userService.FindUserByMobileNumber(payment.ToMobileNumber);

                    if (payee != null)
                    {
                        payment.ToAccount = payee.PaymentAccounts[0];

                        //Put Payment in correct Payment Batch

                        //Send out SMS Message to payee
                        smsService.SendSMS(new DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("You received a payment for {0} from {1}.  The payment is complete. PdThx.me", payment.PaymentAmount, payment.FromMobileNumber),
                            MobileNumber = payment.ToMobileNumber
                        });
                        //Send out SMS Message to payer
                        smsService.SendSMS(new DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("Your payment for {0} to {1} is complete. PdThx.me", payment.PaymentAmount, payment.ToMobileNumber),
                            MobileNumber = payment.ToMobileNumber
                        });
                        //Send out confirmation email to payer
                        emailService.SendEmail(new DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "Confirmation of your payment to " + payment.ToMobileNumber + ".",
                            Body = "Your payment in the amount of " + payment.PaymentAmount + " was delivered to " + payment.ToMobileNumber + ".",
                            FromAddress = fromAddress,
                            ToAddress = payment.FromAccount.User.EmailAddress
                        });
                        emailService.SendEmail(new DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "You received a payment from " + payment.FromMobileNumber + ".",
                            Body = "Your received a payment of " + payment.PaymentAmount + " from " + payment.FromMobileNumber + ". " + payment.Comments,
                            FromAddress = fromAddress,
                            ToAddress = payment.ToAccount.User.EmailAddress
                        });

                        //Update Payment Status
                        payment.PaymentStatus = PaymentStatus.Pending;
                    }
                    else
                    {
                        //Send out SMS Message to payee
                        smsService.SendSMS(new DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("You received a payment request for {0} from {1}.  Go to PdThx.me to complete the transaction.", payment.PaymentAmount,  payment.FromMobileNumber),
                            MobileNumber = payment.ToMobileNumber
                        });
                        //Send out SMS Message to payer
                        smsService.SendSMS(new DataContracts.SMS.SMSRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Message = String.Format("Your payment request for {0} was submitted to an unregistered user {1}. PdThx.me", payment.PaymentAmount, payment.ToMobileNumber),
                            MobileNumber = payment.ToMobileNumber
                        });
                        emailService.SendEmail(new DataContracts.Email.EmailRequest()
                       {
                           ApiKey = payment.Application.ApiKey,
                           Subject = "Confirmation of your payment to " + payment.ToMobileNumber + ".",
                           Body = "Your payment in the amount of " + payment.PaymentAmount + " was delivered to " + payment.ToMobileNumber + ".",
                           FromAddress = fromAddress,
                           ToAddress = payment.FromAccount.User.EmailAddress
                       });
                        emailService.SendEmail(new DataContracts.Email.EmailRequest()
                        {
                            ApiKey = payment.Application.ApiKey,
                            Subject = "Your recent payment to " + payment.ToMobileNumber + ".",
                            Body = String.Format("The recipient of your payment ({0}) does not have an account with PdThx.  We have sent their mobile number information about your payment and instructions to register.  Please help us ensure that your transaction is completed by reminding the recipient of your payment to register with us.  Thanks.", payment.ToMobileNumber),
                            FromAddress = fromAddress,
                            ToAddress = payment.FromAccount.User.EmailAddress
                        });
                    }
                    payment.LastUpdatedDate = System.DateTime.Now;
                    
                    paymentService.UpdatePayment(payment);

                    break;
            }


        }
    }
}