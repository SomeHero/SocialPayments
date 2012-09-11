using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Services.UserProcessors.Interfaces;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices;
using NLog;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using SocialPayments.DomainServices.Interfaces;
using System.Collections.ObjectModel;

namespace SocialPayments.Services.UserProcessors
{
    public class RegisteredUserProcessor: IUserProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        private FormattingServices _formattingService;
        private TransactionBatchService _transactionBatchService;
        private ValidationService _validationService;
        private UserService _userService;
        private ISMSService _smsService;
        private IEmailService _emailService;

        private string _mobileValidationMessage = "Welcome to PdThx.   Your verfication codes are {0} and {1}.";
        private string _templateName = "Welcome/Registration";
        private string _welcomeEmailSubject = "Welcome to PaidThx";
        private string _activationUrl = "http://www.paidthx.com/mobile/confirmation?{0}";

        public RegisteredUserProcessor()
        {
            _ctx = new Context();
            _logger = LogManager.GetCurrentClassLogger();

            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = new SMSService(_ctx);
            _emailService = new EmailService(_ctx);
            _userService = new UserService(_ctx);
        }

        public RegisteredUserProcessor(IDbContext context, IEmailService emailService, ISMSService smsService)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();

            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = smsService;
            _emailService = emailService;
            _userService = new UserService(_ctx);
        }


        public bool Process(Domain.User user)
        {
            _logger.Log(LogLevel.Info, string.Format("Processing Registered User for {0}, Starting.", user.UserId));

            try
            {

                if (!String.IsNullOrEmpty(user.MobileNumber))
                {

                    user.UserStatus = UserStatus.Active;

                    var messages = _ctx.Messages
                        .Where(m => (m.RecipientUri == user.EmailAddress || m.RecipientUri == user.MobileNumber) && m.StatusValue.Equals((int)PaystreamMessageStatus.Processing))
                        .ToList();

                    _logger.Log(LogLevel.Info, string.Format("Processing Registered User for {0}, Found {1} Messages.", user.UserId, messages.Count));

                    var transactionsList = new Collection<Transaction>();

                    foreach (var message in messages)
                    {
                        message.Payment.RecipientAccount = user.PreferredReceiveAccount;
                        transactionsList.Add(_ctx.Transactions.Add(new Domain.Transaction()
                        {
                            Amount = message.Amount,
                            CreateDate = System.DateTime.Now,
                            AccountNumber = user.PreferredReceiveAccount.AccountNumber,
                            RoutingNumber = user.PreferredReceiveAccount.RoutingNumber,
                            NameOnAccount = user.PreferredReceiveAccount.NameOnAccount,
                            AccountType = Domain.AccountType.Checking,
                            Id = Guid.NewGuid(),
                            PaymentChannelType = PaymentChannelType.Single,
                            StandardEntryClass = StandardEntryClass.Web,
                            Status = TransactionStatus.Pending,
                            Type = TransactionType.Deposit
                        }));

                    }

                    _ctx.SaveChanges();

                    _transactionBatchService.AddTransactionsToBatch(transactionsList);

                    _ctx.SaveChanges();

                    //send registration email
                    //string emailSubject = _welcomeEmailSubject;

                    //var replacementElements = new List<KeyValuePair<string, string>>();
                    //replacementElements.Add(new KeyValuePair<string, string>("EMAILADDRESS", user.EmailAddress));
                    //replacementElements.Add(new KeyValuePair<string, string>("LINK_ACTIVATION", String.Format(_activationUrl, user.ConfirmationToken)));

                    //_emailService.SendEmail(user.EmailAddress, emailSubject, _templateName, replacementElements);

                }

                _logger.Log(LogLevel.Info, string.Format("Processing Registered User for {0}. Finished.", user.UserId));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, string.Format("Unhandled Exception Processing Registered User. Exception: {0}", ex.Message));

                var innerException = ex.InnerException;

                while (innerException != null)
                {
                    _logger.Log(LogLevel.Fatal, string.Format("Unhandled Exception Processing Registered User. Inner Exception: {0}", innerException.Message));

                    innerException = innerException.InnerException;
                }
            
            }

            return true;
        }
    }
}
