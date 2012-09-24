using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class ErrorModels
    {
        public class ErrorModel
        {
            public string Message { get; set; }
            public int ErrorCode { get; set; }
        }
    }
}