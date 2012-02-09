using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SocialPayments.Web.Models
{
    public class PaymentModel
    {
        public DateTime PaymentDate { get; set; }
        public string ToMobileNumber { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double PaymentAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}