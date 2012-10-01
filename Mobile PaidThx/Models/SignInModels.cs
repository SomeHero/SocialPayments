using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Models
{
    public class SignInModels
    {
        public class SignInModel
        {
            public string FBState { get; set; }
        }
        public class SecurityQuestionChallengeModel
        {
            public string SecurityQuestion { get; set; }
            public string SecurityQuestionAnswer { get; set; }
        }
    }
}