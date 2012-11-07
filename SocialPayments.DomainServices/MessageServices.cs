using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DataLayer;
using NLog;
using Amazon.SimpleNotificationService;
using System.Configuration;
using Amazon.SimpleNotificationService.Model;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.DomainServices.CustomExceptions;
using System.Threading.Tasks;
using System.Threading;
using MoonAPNS;
using SocialPayments.DomainServices.MessageProcessing;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class MessageServices
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private DomainServices.ValidationService _validationServices = new DomainServices.ValidationService();
        private DomainServices.FormattingServices _formattingServices = new DomainServices.FormattingServices();

        public MessageServices()
        { }
        public void AcceptPledge(string id, string userId, string paymentAccountId, string securityPin)
        {
            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();

                Guid messageId;
                Guid userGuid;

                Domain.Message message = null;
                Domain.User user = null;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                user = ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                Guid.TryParse(id, out messageId);

                if (messageId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pledge {0} Not Found", id));

                message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId && m.RecipientId == user.UserId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pledge {0} Not Found", id));


                if (!(message.Status == PaystreamMessageStatus.SubmittedRequest || message.Status == PaystreamMessageStatus.NotifiedRequest  || message.Status == PaystreamMessageStatus.PendingRequest))
                    throw new CustomExceptions.BadRequestException(String.Format("Unable to Accept Pledge {0}.  Invalid State {1}.", id, message.Status));

                Domain.PaymentAccount paymentAccount = null;
                Guid paymentAccountGuid;

                Guid.TryParse(paymentAccountId, out paymentAccountGuid);

                if (paymentAccountGuid == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account {0}", paymentAccountId));

                paymentAccount = ctx.PaymentAccounts.FirstOrDefault(a => a.Id == paymentAccountGuid);


                if (paymentAccount == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account {0}", paymentAccountId));


                message.Status = PaystreamMessageStatus.AcceptedRequest;
                message.LastUpdatedDate = System.DateTime.Now;

                var paymentMessage = AddMessage(message.ApiKey.ToString(), userId, message.SenderId.ToString(), message.SenderUri, paymentAccount.Id.ToString(),
                                         message.Amount, message.Comments, "Donation", 0, 0, message.senderFirstName, message.senderLastName, message.senderImageUri, securityPin,
                                         message.Id.ToString(), "Standard");

                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    SubmittedDonationMessageTask task = new SubmittedDonationMessageTask();
                    task.Execute(paymentMessage.Id);

                });
            }

        }
        public void RejectPledgeRequest(string id, string userId, string securityPin)
        {
            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();

                Guid messageId;
                Guid userGuid;

                Domain.Message message = null;
                Domain.User user = null;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                user = ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                Guid.TryParse(id, out messageId);

                if (messageId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pledge {0} Not Found", id));

                message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId && m.RecipientId == user.UserId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pledge {0} Not Found", id));

                if (!(message.Status == PaystreamMessageStatus.SubmittedRequest || message.Status == PaystreamMessageStatus.NotifiedRequest || message.Status == PaystreamMessageStatus.PendingRequest))
                    throw new CustomExceptions.BadRequestException(String.Format("Unable to Reject Pledge {0}.  Invalid State {1}.", id, message.Status));

                if (!securityServices.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }
                message.Status = PaystreamMessageStatus.RejectedRequest;
                message.LastUpdatedDate = System.DateTime.Now;

                ctx.SaveChanges();

            }
        }
        public Message Pledge(string apiKey, string originatorId, string organizationId, string recipientUri, double amount, string comments,
            double latitude, double longitude, string recipientFirstName, string recipientLastName, string recipientImageUri, string securityPin)
        {
            Guid applicationGuid;
            Guid originatorGuid;
            Guid organizationGuid;

            Domain.User organization = null;
            Domain.User originator = null;

            var userService = new UserService();
            var securityServices = new SecurityService();

            Guid.TryParse(apiKey, out applicationGuid);
            Guid.TryParse(originatorId, out originatorGuid);
            Guid.TryParse(organizationId, out organizationGuid);

            Domain.MessageType type = MessageType.AcceptPledge;
            Domain.PaystreamMessageStatus status = PaystreamMessageStatus.SubmittedRequest;

            using (var ctx = new Context())
            {

                //if (applicationGuid == null)
                 //   throw new CustomExceptions.BadRequestException(String.Format("Application {0} Not Valid", apiKey));

                Application application = ctx.Applications.First();

                if (application == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Application {0} Not Valid", apiKey));

                originator = ctx.Users.FirstOrDefault(u => u.UserId == originatorGuid);

                if (originator == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Orriginator of Pledge {0} Not Found", originatorId));

                //Check is SENDER is LOCKED OUT
                if (originator.IsLockedOut)
                    throw new CustomExceptions.BadRequestException(String.Format("Sender {0} Is Locked Out", originatorId), 1001);

                //Validate SENDER Security PINSWIPE
                if (!securityServices.Encrypt(securityPin).Equals(originator.SecurityPin))
                {
                    originator.PinCodeFailuresSinceLastSuccess += 1;

                    if (originator.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        originator.IsLockedOut = true;
                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", originator.UserId), 1001);
                    }

                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }

                organization = ctx.Users.FirstOrDefault(u => u.UserId == organizationGuid);

                if (organization == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Organization {0} Not Found", organizationId));

                if (organization.PreferredReceiveAccount == null)
                    throw new CustomExceptions.BadRequestException(String.Format("No Receive Account Specified for Organization {0}", organizationId));

                Domain.Message messageItem = ctx.Messages.Add(new Message()
                {
                    Amount = amount,
                    Application = application,
                    Comments = comments,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    Status = status,
                    WorkflowStatus = PaystreamMessageWorkflowStatus.Pending,
                    MessageType = type,
                    Originator = originator,
                    RecipientUri = recipientUri,
                    SenderUri = organization.Merchant.Name,
                    Sender = organization,
                    SenderId = organization.UserId,
                    SenderAccount = organization.PreferredReceiveAccount,
                    Latitude = latitude,
                    Longitude = longitude,
                    recipientFirstName = (!String.IsNullOrEmpty(recipientFirstName) ? recipientFirstName : null),
                    recipientLastName = (!String.IsNullOrEmpty(recipientLastName) ? recipientLastName : null),
                    recipientImageUri = recipientImageUri,
                    recipientHasSeen = false,
                    senderHasSeen = false
                });

                ctx.SaveChanges();

                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    SubmittedPledgeMessageTask pledgeTask = new SubmittedPledgeMessageTask();
                    pledgeTask.Execute(messageItem.Id);

                });

                return messageItem;

            }

        }
        public Message AddMessage(string apiKey, string senderId, string recipientId, string recipientUri, string senderAccountId, double amount, string comments, string messageType,
             double latitude, double longitude, string recipientFirstName, string recipientLastName, string recipientImageUri, string securityPin,
            string associatedRequestId, string deliveryMethod)
        {
            _logger.Log(LogLevel.Info, String.Format("Adding a Message. {0} to {1} with {2}", senderId, recipientUri, senderAccountId));

            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();
                var formattingServices = new DomainServices.FormattingServices();
                var validationServices = new DomainServices.ValidationService();

                Guid applicationGuid;
                Guid senderGuid;
                User sender = null;
                Guid recipientGuid;
                User recipient = null;
                Message associatedRequest = null;

                if (!(messageType.ToUpper() == "PAYMENT" || messageType.ToUpper() == "PAYMENTREQUEST" || messageType.ToUpper() == "PLEDGE" || messageType.ToUpper() == "DONATION"))
                    throw new BadRequestException("Invalid Message Type.  Message Type must be Payment, PaymentRequest, Pledge or Donation");

                if (!(deliveryMethod.ToUpper() == "STANDARD" || deliveryMethod.ToUpper() == "EXPRESS"))
                    throw new BadRequestException("Delivery Method Must be Standard or Express");

                MessageType type = MessageType.Payment;
                PaystreamMessageStatus status = PaystreamMessageStatus.SubmittedPayment;
                DeliveryMethod methodOfDelivery = DeliveryMethod.Standard;
                double deliveryFeeAmount = 0.0;

                if (messageType.ToUpper() == "PAYMENT")
                {
                    type = MessageType.Payment;
                    status = PaystreamMessageStatus.SubmittedPayment;
                }

                if (messageType.ToUpper() == "PAYMENTREQUEST")
                {
                    type = MessageType.PaymentRequest;
                    status = PaystreamMessageStatus.SubmittedRequest;
                }
                if (messageType.ToUpper() == "PLEDGE")
                {
                    type = MessageType.AcceptPledge;
                    status = PaystreamMessageStatus.SubmittedRequest;
                }
                if (messageType.ToUpper() == "DONATION")
                {
                    type = MessageType.Donation;
                    status = PaystreamMessageStatus.SubmittedPayment;
                }

                if (deliveryMethod.ToUpper() == "EXPRESS")
                {
                    methodOfDelivery = DeliveryMethod.Express;
                }


                //Validate the specified APIKEY is Valid
               // Guid.TryParse(apiKey, out applicationGuid);

               // if (applicationGuid == null)
                   // throw new CustomExceptions.BadRequestException(String.Format("Application {0} Not Valid", apiKey));

                Application application = ctx.Applications.First();

                if (application == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Application {0} Not Valid", apiKey));

                //Validate the SENDER is Valid
                Guid.TryParse(senderId, out senderGuid);

                if (senderGuid == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Sender {0} Not Found", senderId));

                sender = ctx.Users.FirstOrDefault(u => u.UserId == senderGuid);

                if (sender == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Sender {0} Not Found", senderId));

                //Check is SENDER is LOCKED OUT
                if (sender.IsLockedOut)
                    throw new CustomExceptions.BadRequestException(String.Format("Sender {0} Is Locked Out", senderId), 1001);

                //Validate SENDER Security PINSWIPE
                if (!securityServices.Encrypt(securityPin).Equals(sender.SecurityPin))
                {
                    sender.PinCodeFailuresSinceLastSuccess += 1;

                    if (sender.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        sender.IsLockedOut = true;
                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", sender.UserId), 1001);
                    }

                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }

                //if pinswipe is correct, reset lockout time and # of failures
                sender.PinCodeLockOutResetTimeout = null;
                sender.PinCodeFailuresSinceLastSuccess = 0;

                //if the recipientId was specified, validate the RECIPIENT
                if (!String.IsNullOrEmpty(recipientId))
                {
                    Guid.TryParse(recipientId, out recipientGuid);

                    if (recipientGuid == null)
                        throw new CustomExceptions.BadRequestException(String.Format("Recipient {0} Not Found", recipientId));

                    recipient = ctx.Users.FirstOrDefault(u => u.UserId == recipientGuid);

                    if (recipient == null)
                        throw new CustomExceptions.BadRequestException(String.Format("Recipient {0} Not Found", recipientId));
                }
                else
                {
                    URIType recipientUriType = GetURIType(recipientUri);

                    if(recipientUriType == URIType.EmailAddress)
                    {
                        var payPoint = sender.PayPoints.FirstOrDefault(p=>p.URI == recipientUri);

                        if(payPoint != null)
                            throw new CustomExceptions.BadRequestException(String.Format("Sender {0} and Recipient {1} have the same pay points. Unable to Process.", senderId, recipientUri));
                    }
                    if (recipientUriType == URIType.MobileNumber)
                        recipientUri = formattingServices.RemoveFormattingFromMobileNumber(recipientUri);

                    if (recipientUriType == URIType.MobileNumber && validationServices.AreMobileNumbersEqual(sender.MobileNumber, recipientUri))
                        throw new CustomExceptions.BadRequestException(String.Format("Sender {0} and Recipient {1} have the same pay points. Unable to Process.", senderId, recipientUri));

                    if (recipientUriType == URIType.MECode && sender.PayPoints.FirstOrDefault(m => m.URI == recipientUri) != null)
                    {
                        _logger.Log(LogLevel.Info, String.Format("Found recipientUriType 'meCode' for {0}", recipientUri));
                        throw new CustomExceptions.BadRequestException(String.Format("Sender {0} and Recipient {1} have the same pay points. Unable to Process.", senderId, recipientUri));
                    }
                }

                

                //VALIDATE the SENDER PAYMENT ACCOUNT
                PaymentAccount senderAccount = GetAccount(sender, senderAccountId);

                if (senderAccount == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Sender Account {0} Not Valid", senderAccountId));

                Guid associatedRequestGuid;

                Guid.TryParse(associatedRequestId, out associatedRequestGuid);

                if (associatedRequestGuid != null)
                {
                    associatedRequest = ctx.Messages
                        .FirstOrDefault(m => m.Id == associatedRequestGuid);
                }
                //TODO: confirm amount is within payment limits (maybe)

                if (methodOfDelivery == DeliveryMethod.Express && amount > sender.ExpressDeliveryFreeThreshold)
                {
                    deliveryFeeAmount = sender.ExpressDeliveryFeePercentage * amount;
                }

                //Add message
                Domain.Message messageItem = ctx.Messages.Add(new Message()
                    {
                        Amount = amount,
                        Application = application,
                        Comments = comments,
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        Status = status,
                        WorkflowStatus = PaystreamMessageWorkflowStatus.Pending,
                        MessageType = type,
                        MessageTypeValue = (int)type,
                        Recipient = recipient,
                        RecipientUri = recipientUri,
                        SenderUri = sender.UserName,
                        Sender = sender,
                        SenderId = sender.UserId,
                        SenderAccount = senderAccount,
                        SenderAccountId = senderAccount.Id,
                        Latitude = latitude,
                        Longitude = longitude,
                        recipientFirstName = (recipient != null && !String.IsNullOrEmpty(recipient.FirstName) ? recipient.FirstName : recipientFirstName),
                        recipientLastName = (recipient != null && !String.IsNullOrEmpty(recipient.LastName) ? recipient.LastName : recipientLastName),
                        recipientImageUri = recipientImageUri,
                        recipientHasSeen = false,
                        PaymentRequest = associatedRequest,
                        senderHasSeen = true,
                        deliveryMethod = methodOfDelivery,
                        deliveryFeeAmount = deliveryFeeAmount
                    });

                ctx.SaveChanges();

                return messageItem;
            }
        }

        public Domain.Message AddPayment(string apiKey, string senderId, string recientUri, string senderAccountId, double amount, string comments, double latitude,
            double longitude, string recipientFirstName, string recipientLastName, string recipientImageUri, string securityPin, string deliveryMethod)
        {
            using (var ctx = new Context())
            {
                Domain.Message message = AddMessage(apiKey, senderId, "", recientUri, senderAccountId, amount, comments, "Payment", latitude, longitude, recipientFirstName,
                       recipientLastName, recipientImageUri, securityPin, "", deliveryMethod);

                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    SubmittedPaymentMessageTask paymentTask = new SubmittedPaymentMessageTask();
                    paymentTask.Execute(message.Id);
                });

                return message;
            }
        }
        public Domain.Message AddRequest(string apiKey, string senderId, string recientUri, string senderAccountId, double amount, string comments, double latitude,
            double longitude, string recipientFirstName, string recipientLastName, string recipientImageUri, string securityPin)
        {
            using (var ctx = new Context())
            {
                Domain.Message message = AddMessage(apiKey, senderId, "", recientUri, senderAccountId, amount, comments, "PaymentRequest", latitude, longitude, recipientFirstName,
                    recipientLastName, recipientImageUri, securityPin, "", "Standard");

                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    SubmittedRequestMessageTask task = new SubmittedRequestMessageTask();
                    task.Execute(message.Id);
                });

                return message;
            }

        }
        public void CancelPayment(string id, string userId, string securityPin)
        {
            using (var _ctx = new Context())
            {
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices();
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

                Guid messageId;
                Guid userGuid;

                Domain.Message message = null;
                Domain.User user = null;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                user = _ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                Guid.TryParse(id, out messageId);

                if (messageId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                message = _ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId && m.SenderId == user.UserId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                if (message.Status != PaystreamMessageStatus.SubmittedPayment && message.Status != PaystreamMessageStatus.NotifiedPayment && message.Status != PaystreamMessageStatus.ProcessingPayment)
                    throw new CustomExceptions.BadRequestException(String.Format("Payment {0} Cannot be Cancelled.  Invalid State", id));

                if (!_securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        _ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    _ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }
                message.LastUpdatedDate = System.DateTime.Now;
                message.Status = PaystreamMessageStatus.CancelledPayment;

                _ctx.SaveChanges();

                //TODO: Start Domain service to remove from batch and kick out any emails
                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    CancelledPaymentMessageTask task = new CancelledPaymentMessageTask();
                    task.Execute(message.Id);

                });
            }
        }
        public void CancelRequest(string id, string userId, string securityPin)
        {
            using (var _ctx = new Context())
            {
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices();
                DomainServices.FormattingServices _formattingService = new DomainServices.FormattingServices();
                DomainServices.TransactionBatchService _transactionBatchService =
                new DomainServices.TransactionBatchService(_ctx, _logger);
                DomainServices.UserService _userService =
                    new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService _paymentAccountServices =
                    new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

                Guid messageId;
                Guid userGuid;

                Domain.Message message = null;
                Domain.User user = null;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                user = _ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                Guid.TryParse(id, out messageId);

                if (messageId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                message = _ctx.Messages.FirstOrDefault(m => m.Id == messageId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                if (!(message.Status == PaystreamMessageStatus.SubmittedRequest || message.Status == PaystreamMessageStatus.PendingRequest || message.Status == PaystreamMessageStatus.NotifiedRequest))
                    throw new CustomExceptions.BadRequestException(String.Format("Request {0} Cannot be Cancelled.  Invalid State", id));

                if (!_securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        _ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    _ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }

                message.LastUpdatedDate = System.DateTime.Now;
                message.Status = PaystreamMessageStatus.CancelledRequest;

                _ctx.SaveChanges();

                //TODO: Start Domain service to remove from batch and kick out any emails

            }
        }
        public Domain.Message Donate(string apiKey, string senderId, string organizationId, string organizationName, string senderAccountId, double amount,
            string comments, string securityPin)
        {
            using (var ctx = new Context())
            {
                Domain.Message message = AddMessage(apiKey, senderId, organizationId, organizationName, senderAccountId, amount, comments,
                    "Donation", 0.0, 0.0, "", "", "", securityPin, "", "Standard");

                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    SubmittedDonationMessageTask task = new SubmittedDonationMessageTask();
                    task.Execute(message.Id);
                });

                return message;
            }

        }
        public void AcceptPaymentRequest(string id, string userId, string paymentAccountId, string securityPin)
        {
            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();

                Guid messageId;
                Guid userGuid;

                Domain.Message message = null;
                Domain.User user = null;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                user = ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                Guid.TryParse(id, out messageId);

                if (messageId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId && m.RecipientId == user.UserId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));


                if (!(message.Status == PaystreamMessageStatus.SubmittedRequest || message.Status == PaystreamMessageStatus.PendingRequest || message.Status == PaystreamMessageStatus.NotifiedRequest))
                    throw new CustomExceptions.BadRequestException(String.Format("Unable to Accept Request {0}.  Invalid State {1}.", id, message.Status));

                Domain.PaymentAccount paymentAccount = null;
                Guid paymentAccountGuid;

                Guid.TryParse(paymentAccountId, out paymentAccountGuid);

                if (paymentAccountGuid == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account {0}", paymentAccountId));

                paymentAccount = ctx.PaymentAccounts.FirstOrDefault(a => a.Id == paymentAccountGuid);


                if (paymentAccount == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account {0}", paymentAccountId));


                message.Status = PaystreamMessageStatus.AcceptedRequest;
                message.LastUpdatedDate = System.DateTime.Now;

                ctx.SaveChanges();

                var paymentMessage = AddMessage(message.ApiKey.ToString(), userId, message.SenderId.ToString(), message.SenderUri, paymentAccount.Id.ToString(),
                                         message.Amount, message.Comments, "Payment", 0, 0, message.senderFirstName, message.senderLastName, message.senderImageUri, securityPin,
                                         message.Id.ToString(), "Standard");

                //kick off background taskes to lookup recipient, send emails, and bath transactions
                Task.Factory.StartNew(() =>
                {
                    SubmittedPaymentMessageTask task = new SubmittedPaymentMessageTask();
                    task.Execute(paymentMessage.Id);

                });
            }

        }
        public Dictionary<string, User> RouteMessage(List<string> recipientUris)
        {
            using (var ctx = new Context())
            {
                DomainServices.UserService userService = new DomainServices.UserService();
                Dictionary<string, User> matchedUsers = new Dictionary<string, User>();

                foreach (string uri in recipientUris)
                {
                    UserPayPoint userPayPoint = ctx.UserPayPoints
                        .Include("User")
                        .FirstOrDefault(p => p.URI == uri);

                    if (userPayPoint != null && userPayPoint.User != null)
                    {

                        string firstName = userPayPoint.User.FirstName;
                        string lastName = userPayPoint.User.LastName;

                        if (firstName == null && lastName == null)
                        {
                            firstName = "PaidThx";
                            lastName = "User";
                        }
                        else if (firstName == null)
                        {
                            firstName = "";
                        }
                        else if (lastName == null)
                        {
                            lastName = "";
                        }

                        if (!matchedUsers.ContainsKey(userPayPoint.URI))
                            matchedUsers.Add(userPayPoint.URI, userPayPoint.User);
                    }
                }

                return matchedUsers;
            }
        }
        public void RejectPaymentRequest(string id, string userId, string securityPin)
        {
            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();

                Guid messageId;
                Guid userGuid;

                Domain.Message message = null;
                Domain.User user = null;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                user = ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", id));

                Guid.TryParse(id, out messageId);

                if (messageId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId && m.RecipientId == user.UserId);

                if (message == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment {0} Not Found", id));

                if (!(message.Status == PaystreamMessageStatus.SubmittedRequest || message.Status == PaystreamMessageStatus.PendingRequest || message.Status == PaystreamMessageStatus.NotifiedRequest))
                    throw new CustomExceptions.BadRequestException(String.Format("Unable to Reject Request {0}.  Invalid State {1}.", id, message.Status));

                if (!securityServices.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }
                message.Status = PaystreamMessageStatus.RejectedRequest;
                message.LastUpdatedDate = System.DateTime.Now;

                ctx.SaveChanges();

            }
        }
        public void UpdateMessagesSeen(string id, List<string> messageIds)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService _userService =
                       new DomainServices.UserService();

                Guid userId;
                User user;

                Guid.TryParse(id, out userId);

                if (userId == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", id));

                user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", id));


                Message messageSeen = null;
                Guid messageGuid;

                foreach (string messageId in messageIds)
                {
                    try
                    {
                        Guid.TryParse(messageId, out messageGuid);

                        if (messageGuid == null)
                            throw new Exception(String.Format("GUID not found for {0}", messageId));

                        messageSeen = _ctx.Messages.Select(m => m).FirstOrDefault(m => m.Id == messageGuid);

                        if (messageSeen == null)
                            throw new Exception(String.Format("Message {0} Not Found", messageId));

                        if (messageSeen.RecipientId.Equals(user.UserId))
                            messageSeen.recipientHasSeen = true;
                        else if (messageSeen.SenderId.Equals(user.UserId))
                            messageSeen.senderHasSeen = true;

                        _ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, "Unable to update message seen for {0}. {1}", messageId, ex.Message);
                    }
                }

                _userService.UpdateUser(user);
            }
        }
        private PaymentAccount GetAccount(User sender, string id)
        {
            Guid accountId;

            Guid.TryParse(id, out accountId);

            if (accountId == null)
                return null;

            _logger.Log(LogLevel.Info, String.Format("# of accounts {0}", sender.PaymentAccounts.Count));

            foreach (var account in sender.PaymentAccounts)
            {
                if (account.Id == accountId)
                    return account;

            }

            return null;

        }
        public List<Domain.Message> GetOutstandingMessage(User user)
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                var mobileNumber = formattingService.RemoveFormattingFromMobileNumber(user.MobileNumber);

                List<Domain.Message> messages = ctx.Messages
                    .Where(m => (m.RecipientUri == mobileNumber || m.RecipientUri == user.EmailAddress) && (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest)
                        || m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedPayment)))
                    .OrderByDescending(m => m.CreateDate).ToList();

                foreach (var message in messages)
                {
                    message.SenderName = formattingService.FormatUserName(user);
                }

                return messages;
            }

        }
        public List<Domain.Message> GetPagedMessages(string userId, string type, int take, int skip, int page, int pageSize, out int totalRecords)
        {
            using (var ctx = new Context())
            {
                var userService = new UserService(ctx);
                var validationServices = new DomainServices.ValidationService();
                var formattingServices = new DomainServices.FormattingServices();

                Guid userGuid;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));



                List<Domain.Message> messages = new List<Message>();
                totalRecords = 0;

                if (type.ToUpper() == "ALL" || String.IsNullOrEmpty(type))
                {
                    totalRecords = ctx.Messages
                        .Where(m => m.SenderId == userGuid || m.RecipientId.Value == userGuid)
                        .Count();

                    messages = ctx.Messages
                  .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Payment")
                    .Include("PaymentRequest")
                        .Where(m => m.SenderId == userGuid || m.RecipientId.Value == userGuid)
                        .OrderByDescending(m => m.CreateDate)
                        .Skip(skip)
                        .Take(take)
                        .ToList<Message>();
                }
                else if (type.ToUpper() == "SENT")
                {
                    totalRecords = ctx.Messages
                        .Where(m => m.SenderId == userGuid && (m.MessageTypeValue.Equals((int)MessageType.Payment) || m.MessageTypeValue.Equals((int)MessageType.Donation)))
                        .Count();

                    messages = ctx.Messages
                  .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Payment")
                    .Include("PaymentRequest")
                        .Where(m => m.SenderId == userGuid && (m.MessageTypeValue.Equals((int)MessageType.Payment) || m.MessageTypeValue.Equals((int)MessageType.Donation)))
                        .OrderByDescending(m => m.CreateDate)
                        .Skip(skip)
                        .Take(take)
                        .ToList<Message>();
                }
                else if (type.ToUpper() == "RECEIVED")
                {
                    totalRecords = ctx.Messages
                        .Where(m => m.RecipientId.Value == userGuid && (m.MessageTypeValue.Equals((int)MessageType.Payment) || m.MessageTypeValue.Equals((int)MessageType.Donation)))
                        .Count();

                    messages = ctx.Messages
                  .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Payment")
                    .Include("PaymentRequest")
                        .Where(m => m.RecipientId.Value == userGuid && (m.MessageTypeValue.Equals((int)MessageType.Payment) || m.MessageTypeValue.Equals((int)MessageType.Donation)))
                        .OrderByDescending(m => m.CreateDate)
                        .Skip(skip)
                        .Take(take)
                        .ToList<Message>();
                }
                else if (type.ToUpper() == "OTHER")
                {
                    totalRecords = ctx.Messages
                        .Where(m => (m.SenderId == userGuid || m.RecipientId.Value == userGuid) && m.MessageTypeValue.Equals((int)Domain.MessageType.PaymentRequest))
                        .Count();

                    messages = ctx.Messages
                  .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Payment")
                    .Include("PaymentRequest")
                        .Where(m => (m.SenderId == userGuid || m.RecipientId.Value == userGuid) && m.MessageTypeValue.Equals((int)Domain.MessageType.PaymentRequest))
                        .OrderByDescending(m => m.CreateDate)
                        .Skip(skip)
                        .Take(take)
                        .ToList<Message>();
                }

                foreach (var message in messages)
                {
                    message.SenderName = userService.GetSenderName(message.Sender);
                    if (message.Recipient != null)
                        message.RecipientName = userService.GetSenderName(message.Recipient);
                    else
                    {
                        if (validationServices.IsPhoneNumber(message.RecipientUri))
                        {
                            message.RecipientUri = formattingServices.FormatMobileNumber(message.RecipientUri);
                            message.RecipientName = message.RecipientUri;
                        }
                        else if (validationServices.IsFacebookAccount(message.RecipientUri))
                            message.RecipientName = message.recipientFirstName + " " + message.recipientLastName;
                        else
                            message.RecipientName = message.RecipientUri;
                    }

                    message.Direction = "In";

                    if (message.SenderId.Equals(userGuid))
                        message.Direction = "Out";


                    if (message.Direction == "In")
                    {
                        if (message.Sender != null && !String.IsNullOrEmpty(message.Sender.ImageUrl))
                            message.TransactionImageUrl = message.Sender.ImageUrl;
                    }
                    else
                    {
                        if (message.Recipient != null && !String.IsNullOrEmpty(message.Recipient.ImageUrl))
                            message.TransactionImageUrl = message.Recipient.ImageUrl;
                        else
                            message.TransactionImageUrl = message.recipientImageUri;
                    }
                }

                return messages;
            }
        }
        public List<Domain.Message> GetMessages(Guid userId)
        {
            using (var ctx = new Context())
            {
                var userService = new UserService(ctx);
                var validationServices = new DomainServices.ValidationService();
                var formattingServices = new DomainServices.FormattingServices();

                var messages = ctx.Messages
                  .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Payment")
                    .Include("PaymentRequest")
                    .Where(m => m.SenderId == userId || m.RecipientId.Value == userId)
                    .OrderByDescending(m => m.CreateDate)
                    .ToList<Message>();

                foreach (var message in messages)
                {
                    message.SenderName = userService.GetSenderName(message.Sender);
                    if (message.Recipient != null)
                        message.RecipientName = userService.GetSenderName(message.Recipient);
                    else
                    {
                        if (validationServices.IsPhoneNumber(message.RecipientUri))
                        {
                            message.RecipientUri = formattingServices.FormatMobileNumber(message.RecipientUri);
                            message.RecipientName = message.RecipientUri;
                        }
                        else if (validationServices.IsFacebookAccount(message.RecipientUri))
                            message.RecipientName = message.recipientFirstName + " " + message.recipientLastName;
                        else
                            message.RecipientName = message.RecipientUri;
                    }

                    message.Direction = "In";

                    if (message.SenderId.Equals(userId))
                        message.Direction = "Out";


                    if (message.Direction == "In")
                    {
                        if (message.Sender != null && !String.IsNullOrEmpty(message.Sender.ImageUrl))
                            message.TransactionImageUrl = message.Sender.ImageUrl;
                    }
                    else
                    {
                        if (message.Recipient != null && !String.IsNullOrEmpty(message.Recipient.ImageUrl))
                            message.TransactionImageUrl = message.Recipient.ImageUrl;
                        else
                            message.TransactionImageUrl = message.recipientImageUri;
                    }
                }

                return messages;
            }
        }
        public Domain.Message GetMessage(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            if (messageId == null)
                throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Found", id));

            return GetMessage(messageId);
        }
        public Domain.Message GetMessage(Guid messageId)
        {
            using (var ctx = new Context())
            {
                var formattingServices = new DomainServices.FormattingServices();
                var validationServices = new DomainServices.ValidationService();
                var userService = new DomainServices.UserService();

                var message = ctx.Messages
                   .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Include("Sender")
                    .Include("Sender.Merchant")
                    .Include("Sender.FacebookUser")
                    .Include("Sender.UserSocialNetworks")
                    .Include("Payment")
                    .Include("PaymentRequest")
                    .FirstOrDefault(m => m.Id == messageId);

                message.SenderName = userService.GetSenderName(message.Sender);
                if (message.Recipient != null)
                    message.RecipientName = userService.GetSenderName(message.Recipient);
                else
                {
                    if (validationServices.IsPhoneNumber(message.RecipientUri))
                    {
                        message.RecipientUri = formattingServices.FormatMobileNumber(message.RecipientUri);
                        message.RecipientName = message.RecipientUri;
                    }
                    else if (validationServices.IsFacebookAccount(message.RecipientUri))
                        message.RecipientName = message.recipientFirstName + " " + message.recipientLastName;
                    else
                        message.RecipientName = message.RecipientUri;
                }
                message.TransactionImageUrl = message.Sender.ImageUrl;

                return message;
            }
        }
        public URIType GetURIType(string uri)
        {
            var uriType = URIType.MobileNumber;

            if (_validationServices.IsEmailAddress(uri))
                uriType = URIType.EmailAddress;
            else if (_validationServices.IsMECode(uri))
                uriType = URIType.MECode;
            else if (_validationServices.IsFacebookAccount(uri))
                uriType = URIType.FacebookAccount;

            return uriType;
        }

        public int GetNumberOfPaystreamUpdates(User user)
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                var mobileNumber = formattingService.RemoveFormattingFromMobileNumber(user.MobileNumber);

                var lastViewedPaystreamDate = System.DateTime.MinValue;
                if (user.LastViewedPaystream != null)
                    lastViewedPaystreamDate = user.LastViewedPaystream.Value;

                int pendingRequests = ctx.Messages
                    .Where(m => ((m.RecipientId == user.UserId) && ((m.MessageTypeValue.Equals((int)MessageType.Payment) && (m.CreateDate >= lastViewedPaystreamDate))
                    || (m.MessageTypeValue.Equals((int)MessageType.PaymentRequest) && m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest)))))
                    .Select(m => m.Id)
                    .Count();

                return pendingRequests;
            }
        }

        public List<Domain.Message> GetQuickSendPayments(string id)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService();
                var formattingService = new DomainServices.FormattingServices();

                Guid userGuid;

                Guid.TryParse(id, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", id));

                Domain.User user = ctx.Users.FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", id));

                List<Domain.Message> messages = null;

                //TODO: refactor query method
                var allMessages = ctx.Messages
                    .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Where
                    (m => m.SenderId == user.UserId && m.MessageTypeValue.Equals((int)MessageType.Payment))
                    .OrderByDescending(m => m.CreateDate).ToList();
                    
                messages = allMessages
                    .Distinct(new SameRecipientComparer())
                    .Take(6).ToList();

                foreach (var message in messages)
                {
                    if (message.Recipient != null)
                        message.Recipient.SenderName = userService.GetSenderName(message.Recipient);
                }

                return messages;
            }
        }

        public List<Domain.Message> GetNewMessages(User user)
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();

                List<Domain.Message> messages = null;

                if (user.PaymentAccounts.Count > 0)
                {
                    // "New" message cases, WITH A PAYMENT ACCOUNT SETUP
                    // 1 -> Requests received with no action taken on them (status -> SubmittedRequest/Pending?)
                    // 2 -> Payments/Requests that are "unseen". Any status applies.

                    messages = ctx.Messages
                        .Where
                        (m => (
                            (m.RecipientId == user.UserId) &&
                            (
                                    (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest) || m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                                || (m.recipientHasSeen == false)
                            )
                        ))
                        .OrderByDescending(m => m.CreateDate).ToList();
                }
                else
                {
                    // No Bank Account
                    // "New" message cases, **WITHOUT** A PAYMENT ACCOUNT SETUP
                    // All cases above, PLUS:
                    //      -> Payments seen/unseen (doesnt matter) that are waiting for the user to setup a bank account.
                    messages = ctx.Messages
                        .Where
                        (m => (
                            (m.RecipientId == user.UserId) &&
                            (
                                    (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest) || m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                                || (m.recipientHasSeen == false)
                                || (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedPayment))
                            )
                        ))
                        .OrderByDescending(m => m.CreateDate).ToList();
                }

                foreach (var message in messages)
                {
                    message.SenderName = formattingService.FormatUserName(user);
                }

                return messages;
            }
        }
        public int GetNumberOfNewMessages(string userId)
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                Domain.User user = null;
                int count = 0;

                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                user = ctx.Users
                    .Include("PaymentAccounts")
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user.PaymentAccounts.Count > 0)
                {
                    // "New" message cases, WITH A PAYMENT ACCOUNT SETUP
                    // 1 -> Requests received with no action taken on them (status -> SubmittedRequest/Pending?)
                    // 2 -> Payments/Requests that are "unseen". Any status applies.

                    count = ctx.Messages
                        .Count
                        (m => (
                            (m.RecipientId == user.UserId) &&
                            (
                                    (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest) || m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                                || (m.recipientHasSeen == false)
                            )
                        ));
                }
                else
                {
                    // No Bank Account
                    // "New" message cases, **WITHOUT** A PAYMENT ACCOUNT SETUP
                    // All cases above, PLUS:
                    //      -> Payments seen/unseen (doesnt matter) that are waiting for the user to setup a bank account.
                    count = ctx.Messages
                        .Count
                        (m => (
                            (m.RecipientId == userGuid) &&
                            (
                                    (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest) || m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                                || (m.recipientHasSeen == false)
                                || (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedPayment))
                            )
                        ));
                }

                return count;
            }
        }
        public List<Domain.Message> GetPendingMessages(User user)
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                var mobileNumber = formattingService.RemoveFormattingFromMobileNumber(user.MobileNumber);

                List<Domain.Message> messages = null;

                // "Pending" messages are:
                // Messages that you are involved in that are waiting for the recipient to take action.

                messages = ctx.Messages
                    .Where
                    (m => (
                        (m.SenderId == user.UserId && m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                        || (m.SenderId == user.UserId && m.StatusValue.Equals((int)PaystreamMessageStatus.SubmittedPayment))
                        || (m.SenderId == user.UserId && !m.senderHasSeen)
                        || (m.RecipientId == user.UserId && !m.recipientHasSeen)
                    ))
                    .OrderByDescending(m => m.CreateDate).ToList();

                foreach (var message in messages)
                {
                    message.SenderName = formattingService.FormatUserName(user);
                }

                return messages;
            }
        }
        public int GetNumberOfPendingMessages(string userId)
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                Domain.User user = null;
                int count = 0;

                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                user = ctx.Users
                    .Include("PaymentAccounts")
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                count = ctx.Messages.Count(m => (
                        (m.SenderId == user.UserId && m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                        || (m.SenderId == user.UserId && m.StatusValue.Equals((int)PaystreamMessageStatus.SubmittedPayment))
                        || (m.SenderId == user.UserId && !m.senderHasSeen)
                        || (m.RecipientId == user.UserId && !m.recipientHasSeen)
                    ));

                return count;
            }
        }

        public void ExpressPayment(string userId, string id, string senderAccountId, string securityPin)
        {
            using (var ctx = new Context())
            {
                var userService = new UserService(ctx);
                var securityService = new DomainServices.SecurityService();

                Guid userGuid;
                Guid messageGuid;
                Guid senderAccountGuid;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                Guid.TryParse(id, out messageGuid);

                if (messageGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Message {0} Not Valid", id));

                Guid.TryParse(senderAccountId, out senderAccountGuid);

                if (senderAccountGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Sender Account {0} Not Valid", senderAccountId));

                var user = userService.GetUserById(userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var message = GetMessage(messageGuid);
                ctx.Messages.Attach(message);

                var paymentAccount = user.PaymentAccounts
                    .FirstOrDefault(a => a.Id == senderAccountGuid);

                if (paymentAccount == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Sender Account {0} Not Valid", senderAccountId));

                if (!securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;

                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException("Account Locked", 1001);
                    }

                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException("Invalid Security Pin", 1003);
                }

                message.Payment.IsExpressed = true;
                message.Payment.EstimatedDeliveryDate = message.Payment.ExpressDeliveryDate;

                ctx.SaveChanges();

                //Start Post Processing Task to batch fee payment
            }

        }
    }
}
