using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SocialPayments.Domain;

namespace SocialPayments.Web.Admin.Models
{
    public class PaymentModel
    {
        public Guid Id { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string ToMobileNumber { get; set; }
        public string FromMobileNumber { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double Amount { get; set; }
        public string Status { get; set; }
    }
}