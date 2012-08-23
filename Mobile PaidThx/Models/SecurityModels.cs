using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class SecurityModels
    {
        public class ChangePasswordModel
        {
            public String OldPassword { get; set; }
            public String NewPassword { get; set; }
            public String NewPasswordConfirmation { get; set; }
        }
        public class ChangeSecurityPinModel
        {
            public String PinCode { get; set; }
        }
        public class ConfirmSecurityPinModel
        {
            public String PinCode { get; set; }
        }
        public class ForgotSecurityPinModel
        {
            public string SecurityQuestion { get; set; }
            public string SecurityQuestionAnswer { get; set; }
        }
        public class SecurityPinModel
        {
            public string PinCode { get; set; }
        }
    }
}