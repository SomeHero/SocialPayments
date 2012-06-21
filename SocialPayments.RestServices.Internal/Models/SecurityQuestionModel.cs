using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class SecurityQuestionModel
    {
        public class QuestionResponse
        {
            public int Id { get; set; }
            public string Question { get; set; }
            public bool IsActive { get; set; }
        }
    }
}