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
        public class ValidateQuestionRequest
        {
            public string ApiKey { get; set; }
            public Guid userId { get; set; }
            public int questionId { get; set; }
            public string questionAnswer { get; set; }
        }
        public class UpdateQuestionRequest
        {
            public string ApiKey { get; set; }
            public Guid userId { get; set; }
            public int oldQuestionId { get; set; }
            public string oldQuestionAnswer { get; set; }
            public int questionId { get; set; }
            public string questionAnswer { get; set; }
        }
    }
}