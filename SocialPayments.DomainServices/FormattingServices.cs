using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SocialPayments.DomainServices
{
    public class FormattingServices
    {
        public FormattingServices()
        {
        }

        public string FormatMobileNumber(string mobileNumber)
        {
            string tempMobileNumber = mobileNumber;

            if (tempMobileNumber[0] == 1)
                tempMobileNumber = tempMobileNumber.Substring(1, tempMobileNumber.Length - 1);

            tempMobileNumber = tempMobileNumber.Replace("-", "");
            tempMobileNumber = tempMobileNumber.Replace("(", "");
            tempMobileNumber = tempMobileNumber.Replace(")", "");
            tempMobileNumber = tempMobileNumber.Replace(" ", "");

            return tempMobileNumber;
        }
    }
}
