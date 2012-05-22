using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;

namespace SocialPayments.DomainServices
{
    public class FormattingServices
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        public FormattingServices()
        {
        }

        public string RemoveFormattingFromMobileNumber(string mobileNumber)
        {
            _logger.Log(LogLevel.Info, String.Format("Remove Formatting from Mobile Number {0}", mobileNumber));
            string tempMobileNumber = mobileNumber;

            if (tempMobileNumber[0] == 1)
                tempMobileNumber = tempMobileNumber.Substring(1, tempMobileNumber.Length - 1);

            tempMobileNumber = tempMobileNumber.Replace("-", "");
            tempMobileNumber = tempMobileNumber.Replace("(", "");
            tempMobileNumber = tempMobileNumber.Replace(")", "");
            tempMobileNumber = tempMobileNumber.Replace(" ", "");

            return tempMobileNumber;

        }
        public string FormatMobileNumber(string mobileNumber)
        {
            _logger.Log(LogLevel.Info, String.Format("Formatting Mobile Number {0}", mobileNumber));
            string tempMobileNumber = mobileNumber;

            if (tempMobileNumber[0] == 1)
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
    }
}
