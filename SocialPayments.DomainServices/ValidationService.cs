using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace SocialPayments.DomainServices
{
    public class ValidationService
    {
        private DomainServices.FormattingServices formattingServices = new FormattingServices();
        private Logger _logger;

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
    }
}
