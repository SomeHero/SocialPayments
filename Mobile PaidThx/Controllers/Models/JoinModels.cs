using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class JoinModels
    {
        public class JoinModel
        {
            public string UserName { get; set; }
            public PaymentModel Payment { get; set; }
        }
    }
}