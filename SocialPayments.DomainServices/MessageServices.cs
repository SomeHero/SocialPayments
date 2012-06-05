﻿using System;
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
            _applicationServices = new ApplicationService(_context);
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
            _applicationServices = new ApplicationService(_context);
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
            _applicationServices = new ApplicationService(_context);
            _securityServices = new SecurityService();
            _transactionBatchServices = new TransactionBatchService(_context, _logger);
            _userServices = new UserService(_context);
            _amazonNotificationService = amazonNotificationService;
        }
        public Message AddMessage(string apiKey, string senderUri, string recipientUri, string senderAccountId, double amount, string comments, string messageType,
            string securityPin)
        {
            return AddMessage(apiKey, senderUri, recipientUri, senderAccountId, amount, comments, messageType, securityPin, 0, 0,
                "", "", "");
        }
        public Message AddMessage(string apiKey, string senderUri, string recipientUri, string senderAccountId, double amount, string comments, string messageType,
            string securityPin, double latitude, double longitude, string recipientFirstName, string recipientLastName, string recipientImageUri)
        {
            User sender = null;

            URIType recipientUriType = GetURIType(recipientUri);
            URIType senderUriType = GetURIType(senderUri);

            if (recipientUriType == URIType.MobileNumber)
                recipientUri = _formattingServices.RemoveFormattingFromMobileNumber(recipientUri);

            if (senderUriType == URIType.MobileNumber)
                senderUri = _formattingServices.RemoveFormattingFromMobileNumber(senderUri);

            if (!(messageType.ToUpper() == "PAYMENT" || messageType.ToUpper() == "PAYMENTREQUEST"))
                throw new ArgumentException(String.Format("Invalid Message Type.  Message Type must be Payment or PaymentRequest"));

            MessageType type = MessageType.Payment;

            if (messageType.ToUpper() == "PAYMENT")
                type = MessageType.Payment;

            if (messageType.ToUpper() == "PAYMENTREQUEST")
                type = MessageType.PaymentRequest;

            try
            {
                sender = _userServices.GetUser(senderUri);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Debug, String.Format("Exception getting Sender {0}. {1}", senderUri, ex.Message));

                throw new ArgumentException(String.Format("Sender {0} Not Found", senderUri));
            }

            if (sender == null)
            {
                var message = String.Format("Unable to find sender {0}", senderUri);
                _logger.Log(LogLevel.Debug, message);

                throw new Exception(message);
            }

            if (sender.PinCodeLockOutResetTimeout != null && System.DateTime.Now < sender.PinCodeLockOutResetTimeout)
            {
                var message = "This account is temporarily locked.  Please try again later.";
                var exception = new AccountLockedPinCodeFailures(message);
                exception.NumberOfFailures = sender.PinCodeFailuresSinceLastSuccess;
                exception.LockOutInterval = 1;
                exception.TemporaryLockout = true;

                _logger.Log(LogLevel.Info, message);
                _logger.Log(LogLevel.Info, String.Format("{0} - {1}", sender.SecurityPin, _securityServices.Encrypt(securityPin)));

                throw exception;
            }
            if (!sender.SetupSecurityPin || !(sender.SecurityPin.Equals(_securityServices.Encrypt(securityPin))))
            {
                string message = String.Format("Invalid Security Pin");

                AccountLockedPinCodeFailures exception = null; 

                if (sender.SetupSecurityPin) 
                    sender.PinCodeFailuresSinceLastSuccess += 1;

                if (sender.PinCodeFailuresSinceLastSuccess > 15)
                { 
                    message = "Invalid Security Pin.  This account is temporarily locked due to security pin failures.";
                    exception = new AccountLockedPinCodeFailures(message);
                    exception.NumberOfFailures = sender.PinCodeFailuresSinceLastSuccess;
                    exception.TemporaryLockout = true;
                    exception.LockOutInterval = 15;

                    sender.PinCodeLockOutResetTimeout = System.DateTime.Now.AddMinutes(15);
                }

                if (sender.PinCodeFailuresSinceLastSuccess > 10)
                {
                    message = "Invalid Security Pin.  This account is temporarily locked for 10 minutes due to security pin failures.";
                    exception = new AccountLockedPinCodeFailures(message);
                    exception.NumberOfFailures = sender.PinCodeFailuresSinceLastSuccess;
                    exception.TemporaryLockout = true;
                    exception.LockOutInterval = 10; 
                    
                    sender.PinCodeLockOutResetTimeout = System.DateTime.Now.AddMinutes(10);
                }

                if (sender.PinCodeFailuresSinceLastSuccess > 5)
                {
                    message = "Invalid Security Pin.  This account is temporarily locked for 5 minutes due to security pin failures.";
                    exception = new AccountLockedPinCodeFailures(message);
                    exception.NumberOfFailures = sender.PinCodeFailuresSinceLastSuccess;
                    exception.TemporaryLockout = true;
                    exception.LockOutInterval = 5; 
                    
                    sender.PinCodeLockOutResetTimeout = System.DateTime.Now.AddMinutes(5);
                }

                if (sender.PinCodeFailuresSinceLastSuccess > 2)
                {
                    message = "Invalid Security Pin.  This account is temporarily locked for 1 minute due to security pin failures.";
                    exception = new AccountLockedPinCodeFailures(message);
                    exception.NumberOfFailures = sender.PinCodeFailuresSinceLastSuccess;
                    exception.TemporaryLockout = true;
                    exception.LockOutInterval = 1;

                    sender.PinCodeLockOutResetTimeout = System.DateTime.Now.AddMinutes(1);
                }

                _logger.Log(LogLevel.Info, message);
                _logger.Log(LogLevel.Info, String.Format("{0} - {1}", sender.SecurityPin, _securityServices.Encrypt(securityPin)));

                _context.SaveChanges();

                throw exception;
            }

            sender.PinCodeLockOutResetTimeout = null;
            sender.PinCodeFailuresSinceLastSuccess = 0;
            
            if (recipientUriType == URIType.MobileNumber && _validationService.AreMobileNumbersEqual(sender.MobileNumber, recipientUri))
            {
                var message = String.Format("Sender and Recipient are the same");
                _logger.Log(LogLevel.Debug, message);

                throw new InvalidOperationException(message);
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
                var message = String.Format("The senderAccountId is invalid.");
                _logger.Log(LogLevel.Debug, message);

                throw new ArgumentException(message);
            }

            //TODO: validate application in request
            Application application = null;

            try
            {
                application = _applicationServices.GetApplication(apiKey); ;
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

            Domain.User recipient = null;
            if (recipientUriType == URIType.MECode)
            {
                var meCode = _context.MECodes
                     .FirstOrDefault(m => m.MeCode.Equals(recipientUri));

                if (meCode == null)
                    throw new ArgumentException("MECode not found.");

                recipient = meCode.User;
            }

            if (recipientUriType == URIType.EmailAddress)
                recipient = _userServices.FindUserByEmailAddress(recipientUri);
            if (recipientUriType == URIType.MobileNumber)
                recipient = _userServices.FindUserByMobileNumber(recipientUri);
            if (recipientUriType == URIType.FacebookAccount)
                recipient = _userServices.GetUser(recipientUri);
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

            try
            {
                MessageStatus messageStatus = MessageStatus.Submitted;

                messageItem = _context.Messages.Add(new Message()
                {
                    Amount = amount,
                    Application = application,
                    ApiKey = application.ApiKey,
                    Comments = comments,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    MessageStatus = MessageStatus.Pending,
                    MessageStatusValue = (int)messageStatus,
                    MessageType = type,
                    MessageTypeValue = (int)type,
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
                    recipientImageUri = recipientImageUri
                });

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                var message = String.Format("Exception adding message. {0}", ex.Message);

                throw new Exception(message);
            }

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", messageItem.Id.ToString());

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
                message.MessageStatus = MessageStatus.CancelPending;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["MessagePostedTopicARN"], "New Message Received", message.Id.ToString());

        }
        public void AcceptPaymentRequest(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            var message = _context.Messages
                .FirstOrDefault(m => m.Id == messageId);

            if (message == null)
                throw new ArgumentException("Invalid Message Id.", "Id");

            if (!(message.MessageStatus == MessageStatus.Pending || message.MessageStatus == MessageStatus.Submitted))
                throw new Exception("Unable to Cancel Message.  Message is not in Pending Status.");

            try
            {
                message.MessageStatus = MessageStatus.RequestAcceptedPending;
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

            if (!(message.MessageStatus == MessageStatus.Pending || message.MessageStatus == MessageStatus.Submitted))
                throw new Exception("Unable to Cancel Message.  Message is not in Pending Status.");

            try
            {
                message.MessageStatus = MessageStatus.RequestRejected;
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

            foreach (var account in sender.PaymentAccounts)
            {
                if (account.Id == accountId)
                    return account;

            }

            return null;

        }


        public Domain.Message GetMessage(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            if (messageId == null)
                throw new Exception("Invalid Message Id");

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
    }
}
