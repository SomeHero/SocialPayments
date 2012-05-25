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

namespace SocialPayments.Services.UserProcessors
{
    public class SubmittedUserProcessor: IUserProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        private FormattingServices _formattingService;
        private TransactionBatchService _transactionBatchService;
        private ValidationService _validationService;
        private UserService _userService;
        private SMSService _smsService;
        private EmailService _emailService;

        private string _mobileValidationMessage = "Welcome to PdThx.   Your verfication codes are {0} and {1}.";
        private string _templateName = "Welcome/Registration";
        private string _welcomeEmailSubject = "Welcome to PaidThx";
        private string _activationUrl = "http://www.paidthx.com/mobile/confirmation?{0}";

        public SubmittedUserProcessor()
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

        public SubmittedUserProcessor(IDbContext context)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();

            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = new SMSService(_ctx);
            _emailService = new EmailService(_ctx);
            _userService = new UserService(_ctx);
        }


        public bool Process(Domain.User user)
        {
            _logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Starting.", user.UserId));

            if (!String.IsNullOrEmpty(user.MobileNumber))
            {
                _logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Sending Verification Codes.", user.UserId));

                //get random alpha numerics
                user.MobileVerificationCode1 = "1234";
                user.MobileVerificationCode2 = "4321";

                user.UserStatus = UserStatus.Pending;

                _ctx.SaveChanges();

                string message = String.Format(_mobileValidationMessage, user.MobileVerificationCode1, user.MobileVerificationCode2);

                //sms mobile verification codes
                _smsService.SendSMS(user.ApiKey, user.MobileNumber, message);

                //send registration email
                string emailSubject = _welcomeEmailSubject;

                var replacementElements = new List<KeyValuePair<string, string>>();
                replacementElements.Add(new KeyValuePair<string, string>("EMAILADDRESS", user.EmailAddress));
                replacementElements.Add(new KeyValuePair<string, string>("LINK_ACTIVATION", String.Format(_activationUrl, user.ConfirmationToken)));
                
                _emailService.SendEmail(user.EmailAddress, emailSubject, _templateName, replacementElements);

            }

            _logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Finished.", user.UserId));

            return true;
        }
    }
}
