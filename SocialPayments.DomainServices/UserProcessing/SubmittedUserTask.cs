using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Configuration;

namespace SocialPayments.DomainServices.UserProcessing
{
    public class SubmittedUserTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string _mobileValidationMessage = "Welcome to PdThx.   Your verfication codes are {0} and {1}.";
        private string _templateName = "Welcome/Registration";
        private string _welcomeEmailSubject = "Welcome to PaidThx";
        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSetURL"];

        public void Execute(Guid userId)
        {
            _logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Starting.", userId));

            using (var ctx = new Context())
            {
                var userService = new UserService(ctx);
                var smsService = new SMSService(ctx);
                var emailService = new EmailService(ctx);

                var user = userService.GetUserById(userId);


                //SendEmailVerificationLink(emailAddressPayPoint);

                //if (mobileNumberPayPoint != null)
                    //SendMobileVerificationCode(mobileNumberPayPoint);

                if (!String.IsNullOrEmpty(user.MobileNumber))
                {
                    _logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Sending Verification Codes.", user.UserId));

                    //get random alpha numerics
                    user.MobileVerificationCode1 = "1234";
                    user.MobileVerificationCode2 = "4321";

                    user.UserStatus = UserStatus.Registered;

                    ctx.SaveChanges();

                    string message = String.Format(_mobileValidationMessage, user.MobileVerificationCode1, user.MobileVerificationCode2);

                    //sms mobile verification codes
                    smsService.SendSMS(user.ApiKey, user.MobileNumber, message);
                  }

                //send registration email
                string emailSubject = _welcomeEmailSubject;

                var replacementElements = new List<KeyValuePair<string, string>>();
                replacementElements.Add(new KeyValuePair<string, string>("EMAILADDRESS", user.EmailAddress));
                replacementElements.Add(new KeyValuePair<string, string>("LINK_ACTIVATION", String.Format("{0}confirmation/{1}", _mobileWebSiteUrl, user.ConfirmationToken)));

                emailService.SendEmail(user.EmailAddress, emailSubject, _templateName, replacementElements);

               

                _logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Finished.", user.UserId));
            }
        }
    }
}
