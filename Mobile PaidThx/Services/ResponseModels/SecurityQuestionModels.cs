using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.ResponseModels
{
    public class SecurityQuestionModels
    {
        public class SecurityQuestionResponse
        {
            public int Id { get; set; }
            public string Question { get; set; }
            public bool IsActive { get; set; }
        }
        public class SetupQuestionRequest
        {
            public int questionId { get; set; }
            public string questionAnswer { get; set; }
        }
        public class ValidateQuestionRequest
        {
            public string questionAnswer { get; set; }
        }
        public class UpdateQuestionRequest
        {
            public Guid userId { get; set; }
            public int oldQuestionId { get; set; }
            public string oldQuestionAnswer { get; set; }
            public int questionId { get; set; }
            public string questionAnswer { get; set; }
        }
    }
}