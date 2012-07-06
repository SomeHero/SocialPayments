using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices
{
    public class FormattingServices
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private static Regex digitsOnly = new Regex(@"[^\d]");

        public FormattingServices()
        {
        }

        public string RemoveFormattingFromMobileNumber(string mobileNumber)
        {
            _logger.Log(LogLevel.Info, String.Format("Remove Formatting from Mobile Number {0}", mobileNumber));

            if (String.IsNullOrEmpty(mobileNumber))
                return "";
            
            string tempMobileNumber = digitsOnly.Replace(mobileNumber, "");

            if (tempMobileNumber[0] == '1')
                tempMobileNumber = tempMobileNumber.Substring(1, tempMobileNumber.Length - 1);

            _logger.Log(LogLevel.Info, String.Format("Remove Formatting from Mobile Number Result {0}", tempMobileNumber));

            return tempMobileNumber;

        }
        public string FormatMobileNumber(string mobileNumber)
        {
            if (String.IsNullOrEmpty(mobileNumber))
                return "";

            _logger.Log(LogLevel.Info, String.Format("Formatting Mobile Number {0}", mobileNumber));
            string tempMobileNumber = digitsOnly.Replace(mobileNumber, "");

            if (String.IsNullOrEmpty(tempMobileNumber))
                return "";

            if (tempMobileNumber[0] == '1')
                tempMobileNumber = tempMobileNumber.Substring(1, tempMobileNumber.Length - 1);

            tempMobileNumber = tempMobileNumber.Replace("-", "");
            tempMobileNumber = tempMobileNumber.Replace("(", "");
            tempMobileNumber = tempMobileNumber.Replace(")", "");
            tempMobileNumber = tempMobileNumber.Replace(" ", "");

            if (tempMobileNumber.Length >= 10)
            {
                tempMobileNumber = String.Format("({0}) {1}-{2}", tempMobileNumber.Substring(0, 3),
                    tempMobileNumber.Substring(3, 3),
                    tempMobileNumber.Substring(6, 4));
            }

            return tempMobileNumber;
        }
        public string FormatUserName(User sender)
        {
            _logger.Log(LogLevel.Debug, String.Format("Getting UserName {0}", sender.UserId));

            if (!String.IsNullOrEmpty(sender.FirstName) || !String.IsNullOrEmpty(sender.LastName))
                return sender.FirstName + " " + sender.LastName;

            if (!String.IsNullOrEmpty(sender.SenderName))
                return sender.SenderName;

            if (!String.IsNullOrEmpty(sender.MobileNumber))
                return FormatMobileNumber(sender.MobileNumber);

            if (!String.IsNullOrEmpty(sender.EmailAddress))
                return sender.EmailAddress;

            return "PaidThx User";


        }

        public string FormatDateTimeForJSON(DateTime? input)
        {
            if (input == null)
                return "";

            return input.Value.ToString("ddd MMM dd HH:mm:ss zzz yyyy");
        }
    }
}
