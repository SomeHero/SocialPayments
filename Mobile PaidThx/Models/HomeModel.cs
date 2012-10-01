using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Mobile_PaidThx.Models
{
    public class IndexModel
    {
        public string Sender { get; set; }
        public string MobileNumber { get; set; }
        public Double Amount { get; set; }
        public string Comments { get; set; }
    }
    public class HomeModel
    {
        public ContactUsModel ContactUsModel { get; set; }
    }

    public class HomeScreenModel
    {
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Score { get; set; }
    }

    public class ContactUsModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Message { get; set; }

        public bool MessageSubmitted { get; set; }
    }
}