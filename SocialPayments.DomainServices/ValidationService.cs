using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Text.RegularExpressions;

namespace SocialPayments.DomainServices
{
    public class ValidationService
    {
        private DomainServices.FormattingServices formattingServices = new FormattingServices();
        private Logger _logger;

        public ValidationService()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }
        public ValidationService(Logger logger)
        {
            _logger = logger;
        }
        public bool AreMobileNumbersEqual(string mobileNumber, string mobileNumberToCompare)
        {
            try
            {
                return formattingServices.FormatMobileNumber(mobileNumber).Equals(formattingServices.FormatMobileNumber(mobileNumberToCompare), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            { 
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Comparing Mobile Number {0}", ex.Message));
            }

            return false;
        }
        public bool IsEmailAddress(string uri)
        {

            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                  + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                  + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                  + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                  + @"[a-zA-Z]{2,}))$";

            Regex reStrict = new Regex(patternStrict);


            bool isStrictMatch = reStrict.IsMatch(uri);

            return isStrictMatch;

        }

        public bool IsPhoneNumber(string uri)
        {
            if (uri.Length >= 3 && uri.Substring(0, 3).Equals("fb_"))
                return false;

            string patternStrict = "[2-9][0-9]{2}[2-9][0-9]{2}[0-9]{4}";
            Regex reStrict = new Regex(patternStrict);
            return reStrict.IsMatch(Regex.Replace(uri, "[^0-9]", ""));           
        }
        public bool IsMECode(string uri)
        {
            if (String.IsNullOrEmpty(uri))
                return false;

            return uri[0].Equals('$');
        }

        public bool IsFacebookAccount(string uri)
        {
            if (uri.Length < 4)
                return false;

            if (uri.Substring(0, 3).Equals("fb_"))
                return true;

            return false;
        }
    }
}
