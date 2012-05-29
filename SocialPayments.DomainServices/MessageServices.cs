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
        private Logger _logger;

        public MessageServices()
        {
            _context = new Context();
            _logger = LogManager.GetCurrentClassLogger();
            _validationService = new ValidationService(_logger);
            _formattingServices = new FormattingServices();
            _applicationServices = new ApplicationService(_context);
            _securityServices = new SecurityService();
            _userServices = new UserService(_context);
        }
        public MessageServices(IDbContext context)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();
            _validationService = new ValidationService(_logger);
            _formattingServices = new FormattingServices();
            _applicationServices = new ApplicationService(_context);
            _securityServices = new SecurityService();
            _userServices = new UserService(_context);
        }
        public Message AddMessage(string apiKey, string senderUri, string recipientUri, string senderAccountId, double amount, string comments, string messageType,
            string securityPin)
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

            if (messageType == "PAYMENTREQUEST")
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

            if(!sender.SetupSecurityPin || !(sender.SecurityPin.Equals(_securityServices.Encrypt(securityPin))))
            {
                var message = String.Format("Invalid Security Pin");
                _logger.Log(LogLevel.Info, message);

                throw new Exception(message);
            }

            if (recipientUriType == URIType.MobileNumber && _validationService.AreMobileNumbersEqual(sender.MobileNumber, recipientUri))
            {
                var message =  String.Format("Sender and Recipient are the same");
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

                throw new ArgumentException(message);
            }

            if (recipientUriType == URIType.MECode)
            {
                var meCode = _context.MECodes
                     .FirstOrDefault(m => m.MeCode.Equals(recipientUri));

                if (meCode == null)
                    throw new ArgumentException("MECode not found.", "recipientUri");
            }

            //TODO: confirm recipient is valid???

            //TODO: confirm amount is within payment limits

            //TODO: try to add message
            Domain.Message messageItem;

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
                     SenderAccountId = senderAccount.Id
                 });

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                var message = String.Format("Exception adding message. {0}", ex.Message);

                throw new Exception(message);
            }

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

            if (message.MessageStatus != MessageStatus.Pending)
                throw new Exception("Unable to Cancel Message.  Message is not in Pending Status.");

            foreach (var transaction in message.Transactions)
            {
                transaction.Status = TransactionStatus.Cancelled;
                //transaction.DateCancelled = System.DateTime;
            }

            _context.SaveChanges();

            //remove from batch


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
   
    }
}
