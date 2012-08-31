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
        private IDbContext _context;
        private ValidationService _validationService;
        private FormattingServices _formattingServices;
        private ApplicationService _applicationServices;
        private SecurityService _securityServices;
        private UserService _userServices;
        private TransactionBatchService _transactionBatchServices;
        private static IAmazonNotificationService _amazonNotificationService;
        private Logger _logger;

        public MessageServices()
        {
            _context = new Context();
            _logger = LogManager.GetCurrentClassLogger();
            _validationService = new ValidationService(_logger);
            _formattingServices = new FormattingServices();
            _applicationServices = new ApplicationService();
            _securityServices = new SecurityService();
            _transactionBatchServices = new TransactionBatchService(_context, _logger);
            _userServices = new UserService(_context);
            _amazonNotificationService = new DomainServices.AmazonNotificationService();
        }
        public MessageServices(IDbContext context)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();

            _validationService = new ValidationService(_logger);
            _formattingServices = new FormattingServices();
            _applicationServices = new ApplicationService();
            _securityServices = new SecurityService();
            _transactionBatchServices = new TransactionBatchService(_context, _logger);
            _userServices = new UserService(_context);
            _amazonNotificationService = new AmazonNotificationService();
        }
        public MessageServices(IDbContext context, IAmazonNotificationService amazonNotificationService)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();

            _validationService = new ValidationService(_logger);
            _formattingServices = new FormattingServices();
            _applicationServices = new ApplicationService();
            _securityServices = new SecurityService();
            _transactionBatchServices = new TransactionBatchService(_context, _logger);
            _userServices = new UserService(_context);
            _amazonNotificationService = amazonNotificationService;
        }
        public Message Donate(string apiKey, string senderId, string organizationId, string senderAccountId, double amount, string comments, string messageType)
        {
            var organization = _userServices.GetUserById(organizationId);

            if (organization == null)
                throw new Exception(String.Format("Unable to find organization {0}", organizationId));

            return AddMessage(apiKey, senderId, organization.UserId.ToString(), organization.Merchant.Name, senderAccountId, amount, comments, "Payment");
        }
        public Message AcceptPledge(string apiKey, User sender, string onBehalfOfId, string recipientUri, double amount, string comments, string messageType)
        {
            var onBehalfOf = _userServices.GetUserById(onBehalfOfId);

            if (onBehalfOf.PreferredReceiveAccount == null)
                throw new Exception("Invalid Preferred Receive Account");

            return AddMessage(apiKey, onBehalfOfId, "", recipientUri, onBehalfOf.PreferredReceiveAccount.Id.ToString(), amount, comments, "Pledge");
        }
        public Message AddMessage(string apiKey, string senderId, string recipientId, string recipientUri, string senderAccountId, double amount, string comments, string messageType)
        {
            return AddMessage(apiKey, senderId, recipientId, recipientUri, senderAccountId, amount, comments, messageType, 0, 0,
                "", "", "");
        }
        public Message AddMessage(string apiKey, string senderId, string recipientId, string recipientUri, string senderAccountId, double amount, string comments, string messageType,
             double latitude, double longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            _logger.Log(LogLevel.Info, String.Format("Adding a Message. {0} to {1} with {2}", senderId, recipientUri, senderAccountId));

            User sender = null;
            User recipient = null;

            if (!(messageType.ToUpper() == "PAYMENT" || messageType.ToUpper() == "PAYMENTREQUEST" || messageType.ToUpper() == "PLEDGE"))
                throw new ArgumentException(String.Format("Invalid Message Type.  Message Type must be Payment or PaymentRequest"));

            MessageType type = MessageType.Payment;
            PaystreamMessageStatus status = PaystreamMessageStatus.SubmittedPayment;

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
                status = PaystreamMessageStatus.SubmittedPledge;
            }

            try
            {
                sender = _userServices.GetUserById(senderId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Debug, String.Format("Exception getting Sender {0}. {1}", senderId, ex.Message));

                throw new ArgumentException(String.Format("Sender {0} Not Found", senderId));
            }

            if (sender == null)
            {
                var message = String.Format("Unable to find sender {0}", senderId);
                _logger.Log(LogLevel.Debug, message);

                throw new Exception(message);
            }

            // Check for user locked out of account.. return failed and with custom PinCode Failures Exception.
            if (sender.IsLockedOut)
            {
                var message = String.Format("Sender {0} is currenty locked out, payment failed.", senderId);
                _logger.Log(LogLevel.Error, message);

                throw new AccountLockedPinCodeFailures(message);
            }

            sender.PinCodeLockOutResetTimeout = null;
            sender.PinCodeFailuresSinceLastSuccess = 0;

            if (!String.IsNullOrEmpty(recipientId))
            {
                recipient = _userServices.GetUserById(recipientId);

                if (recipient == null)
                {
                    var message = String.Format("Recipeint {0} Not Found", recipientId);
                    _logger.Log(LogLevel.Info, message);

                    throw new Exception(message);
                }
            }
            else
            {
                URIType recipientUriType = GetURIType(recipientUri);

                if (recipientUriType == URIType.MobileNumber)
                    recipientUri = _formattingServices.RemoveFormattingFromMobileNumber(recipientUri);

                if (recipientUriType == URIType.MobileNumber && _validationService.AreMobileNumbersEqual(sender.MobileNumber, recipientUri))
                {
                    var message = String.Format("Sender and Recipient are the same");
                    _logger.Log(LogLevel.Debug, message);

                    throw new InvalidOperationException(message);
                }
            }

            PaymentAccount senderAccount = null;

            try
            {
                senderAccount = GetAccount(sender, senderAccountId);
            }
            catch (Exception ex)
            {
                var message = String.Format("Exception getting Sender Account {0}. {1}", senderAccountId, ex.Message);

                _logger.Log(LogLevel.Debug, message);

                throw new Exception(message);
            }

            if (senderAccount == null)
            {
                var message = String.Format("The senderAccountId {0} is invalid.", senderAccountId);
                _logger.Log(LogLevel.Debug, message);

                throw new ArgumentException(message);
            }

            //TODO: validate application in request
            Application application = null;

            try
            {
                application = _applicationServices.GetApplication(apiKey); ; // Is this an error James?
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Debug, String.Format("Exception getting application {0}. {1}", apiKey, ex.Message));

                throw ex;
            }

            if (application == null)
            {
                var message = String.Format("Application {0} was not found.", apiKey);

                _logger.Log(LogLevel.Debug, String.Format("Exception getting application {0}. {1}", apiKey, message));

                throw new InvalidOperationException(message);
            }

            //TODO: confirm recipient is valid???

            //TODO: confirm amount is within payment limits

            //TODO: try to add message
            Domain.Message messageItem;

            _logger.Log(LogLevel.Info, "Adding Message");

            if (sender == null || senderAccount == null)
            {
                _logger.Log(LogLevel.Info, "Sender or Sender Account Not Set");
                throw new Exception("Sender or Sender Account Not Set");
            }

            var senderUri = sender.UserName;

            try
            {
                messageItem = _context.Messages.Add(new Message()
                {
                    Amount = amount,
                    Application = application,
                    ApiKey = application.ApiKey,
                    Comments = comments,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    Status = status,
                    WorkflowStatus = PaystreamMessageWorkflowStatus.Pending,
                    MessageType = type,
                    MessageTypeValue = (int)type,
                    Recipient = recipient,
                    RecipientUri = recipientUri,
                    SenderUri = senderUri,
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
                    senderHasSeen = true
                });

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                var message = String.Format("Exception adding message. {0}", ex.Message);

                throw new Exception(message);
            }

            //_amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", messageItem.Id.ToString());

            Task.Factory.StartNew(() =>
            {
                _logger.Log(LogLevel.Info, String.Format("Started Summitted {0} Task. {1} to {2}", messageType, recipientUri, senderUri));

                switch (messageItem.MessageType)
                {
                    case MessageType.Payment:
                        SubmittedPaymentMessageTask paymentTask = new SubmittedPaymentMessageTask();
                        paymentTask.Execute(messageItem.Id);

                        break;

                    case MessageType.PaymentRequest:
                        SubmittedRequestMessageTask requestTask = new SubmittedRequestMessageTask();
                        requestTask.Execute(messageItem.Id);

                        break;

                    case MessageType.AcceptPledge:
                        SubmittedPledgeMessageTask pledgeTask = new SubmittedPledgeMessageTask();
                        pledgeTask.Execute(messageItem.Id);

                        break;
                }

            }).ContinueWith(task =>
            {
                _logger.Log(LogLevel.Info, String.Format("Completed Summitted Message Task. {0} to {1}", recipientUri, senderUri));
            });

            _logger.Log(LogLevel.Info, String.Format("Completed Adding a Message. {0} to {1}", recipientUri, senderUri));

            return messageItem;
        }
        public void CancelMessage(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            var message = _context.Messages
                .FirstOrDefault(m => m.Id == messageId);

            if (message == null)
                throw new ArgumentException("Invalid Message Id.", "Id");

            try
            {
                message.LastUpdatedDate = System.DateTime.Now;
                if (message.MessageType == MessageType.Payment)
                    message.Status = PaystreamMessageStatus.CancelledPayment;
                else
                    message.Status = PaystreamMessageStatus.CancelledRequest;

                message.WorkflowStatus = PaystreamMessageWorkflowStatus.Pending;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", id);

        }
        public void AcceptPaymentRequest(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            var message = _context.Messages
                .FirstOrDefault(m => m.Id == messageId);

            if (message == null)
                throw new ArgumentException("Invalid Message Id.", "Id");

            if (!(message.Status == PaystreamMessageStatus.NotifiedRequest))
                throw new Exception("Unable to Cancel Message.  Message is not in Pending Status.");

            try
            {
                message.Status = PaystreamMessageStatus.AcceptedRequest;
                message.LastUpdatedDate = System.DateTime.Now;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", message.Id.ToString());

        }
        public void RejectPaymentRequest(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            var message = _context.Messages
                .FirstOrDefault(m => m.Id == messageId);

            if (message == null)
                throw new ArgumentException("Invalid Message Id.", "Id");

            if (!(message.Status == PaystreamMessageStatus.PendingRequest))
                throw new Exception("Unable to Cancel Message.  Message is not in Pending Status.");

            try
            {
                message.Status = PaystreamMessageStatus.RejectedRequest;
                message.LastUpdatedDate = System.DateTime.Now;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", message.Id.ToString());

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
            var formattingService = new DomainServices.FormattingServices();
            var mobileNumber = formattingService.RemoveFormattingFromMobileNumber(user.MobileNumber);

            List<Domain.Message> messages = _context.Messages
                .Where(m => (m.RecipientUri == mobileNumber || m.RecipientUri == user.EmailAddress) && (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest)
                    || m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedPayment)))
                .OrderByDescending(m => m.CreateDate).ToList();

            foreach (var message in messages)
            {
                message.SenderName = formattingService.FormatUserName(user);
            }

            return messages;

        }
        public List<Domain.Message> GetPagedMessages(int take, int skip, int page, int pageSize, out int totalRecords)
        {
            using (var ctx = new Context())
            {
                totalRecords = ctx.Messages.Count();

                return ctx.Messages.Select(m => m)
                    .OrderByDescending(m => m.CreateDate)
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }
        }
        public List<Domain.Message> GetMessages(Guid userId)
        {
            Domain.User user;

            var messages = _context.Messages
                .Include("Recipient")
                .Where(m => m.SenderId == userId || m.RecipientId.Value == userId)
                .OrderByDescending(m => m.CreateDate)
                .ToList<Message>();

            URIType senderUriType = URIType.MECode;
            URIType recipientUriType = URIType.MECode;

            string senderName = "";
            string recipientName = "";

            foreach (var message in messages)
            {
                senderUriType = URIType.MECode;
                recipientUriType = URIType.MECode;

                senderUriType = GetURIType(message.SenderUri);
                recipientUriType = GetURIType(message.RecipientUri);

                if (!String.IsNullOrEmpty(message.Sender.FirstName) || !String.IsNullOrEmpty(message.Sender.LastName))
                    senderName = message.Sender.FirstName + " " + message.Sender.LastName;
                else if (!String.IsNullOrEmpty(message.senderFirstName) || !String.IsNullOrEmpty(message.senderLastName))
                    senderName = message.senderFirstName + " " + message.Sender.LastName;
                else
                    senderName = (senderUriType == URIType.MobileNumber ? _formattingServices.FormatMobileNumber(message.SenderUri) : message.SenderUri);

                if (message.Recipient != null && (!String.IsNullOrEmpty(message.Recipient.FirstName) || !String.IsNullOrEmpty(message.Recipient.LastName)))
                    recipientName = message.Recipient.FirstName + " " + message.Recipient.LastName;
                else if (!String.IsNullOrEmpty(message.recipientFirstName) || !String.IsNullOrEmpty(message.recipientLastName))
                    recipientName = message.recipientFirstName + " " + message.recipientLastName;
                else
                    recipientName = (recipientUriType == URIType.MobileNumber ? _formattingServices.FormatMobileNumber(message.RecipientUri) : message.RecipientUri);

                message.SenderName = senderName;
                message.RecipientName = recipientName;
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
        public Domain.Message GetMessage(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            if (messageId == null)
                throw new Exception("Invalid Message Id");

            return GetMessage(messageId);
        }
        public Domain.Message GetMessage(Guid messageId)
        {
            var message = _context.Messages
                .FirstOrDefault(m => m.Id == messageId);

            if (message == null)
                throw new Exception("Invalid Message Id.");

            return message;
        }
        public URIType GetURIType(string uri)
        {
            var uriType = URIType.MobileNumber;

            if (_validationService.IsEmailAddress(uri))
                uriType = URIType.EmailAddress;
            else if (_validationService.IsMECode(uri))
                uriType = URIType.MECode;
            else if (_validationService.IsFacebookAccount(uri))
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

                int pendingRequests = _context.Messages
                    .Where(m => ((m.RecipientId == user.UserId) && ((m.MessageTypeValue.Equals((int)MessageType.Payment) && (m.CreateDate >= lastViewedPaystreamDate))
                    || (m.MessageTypeValue.Equals((int)MessageType.PaymentRequest) && m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest)))))
                    .Select(m => m.Id)
                    .Count();

                return pendingRequests;
            }
        }

        public List<Domain.Message> GetQuickSendPayments(User user)
        {
            var formattingService = new DomainServices.FormattingServices();

            List<Domain.Message> messages = null;
            
            messages = _context.Messages
                .Where
                (m => m.SenderId == user.UserId && m.MessageTypeValue.Equals((int)MessageType.Payment))
                .OrderByDescending(m => m.CreateDate).ToList();

            // Not sure what distinct does, but maybe it's unique entries?
            return messages.Distinct(new SameRecipientComparer()).Take(6).ToList();
        }

        public List<Domain.Message> GetNewMessages(User user)
        {
            var formattingService = new DomainServices.FormattingServices();

            List<Domain.Message> messages = null;

            if (user.PaymentAccounts.Count > 0)
            {
                // "New" message cases, WITH A PAYMENT ACCOUNT SETUP
                // 1 -> Requests received with no action taken on them (status -> SubmittedRequest/Pending?)
                // 2 -> Payments/Requests that are "unseen". Any status applies.

                messages = _context.Messages
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
                messages = _context.Messages
                    .Where
                    (m => (
                        ( m.RecipientId == user.UserId ) &&
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

        public List<Domain.Message> GetPendingMessages(User user)
        {
            var formattingService = new DomainServices.FormattingServices();
            var mobileNumber = formattingService.RemoveFormattingFromMobileNumber(user.MobileNumber);

            List<Domain.Message> messages = null;

            // "Pending" messages are:
            // Messages that you are involved in that are waiting for the recipient to take action.

            messages = _context.Messages
                .Where
                (m => (
                    ( m.SenderId == user.UserId && m.StatusValue.Equals((int)PaystreamMessageStatus.PendingRequest))
                    || ( m.SenderId == user.UserId && m.StatusValue.Equals((int)PaystreamMessageStatus.SubmittedPayment))
                    || ( m.SenderId == user.UserId && ! m.senderHasSeen )
                    || ( m.RecipientId == user.UserId && ! m.recipientHasSeen )
                ))
                .OrderByDescending(m => m.CreateDate).ToList();

            foreach (var message in messages)
            {
                message.SenderName = formattingService.FormatUserName(user);
            }

            return messages;
        }
    }
}
