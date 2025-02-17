﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Mobile_PaidThx.Models
{
        public class RegisterModel
        {
            [Required]
            [DataType(DataType.EmailAddress)]
            [Display(Name = "Email address")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public MessageModel Payment { get; set; }
        }
        public class MobileDeviceVerificationModel
        {
            [Required]
            [Display(Name = "Verification Code")]
            public string VerificationCode { get; set; }

            public MessageModel Payment { get; set; }
            public String MobileNumber { get; set; }
        }
        public class SetupACHAccountModel
        {
            [Required]
            [Display(Name = "Name On Account")]
            public string NameOnAccount { get; set; }

            [Required]
            [Display(Name = "Routing Number")]
            public string RoutingNumber { get; set; }

            [Required]
            [Display(Name = "Account Number")]
            public string AccountNumber { get; set; }

            [Required]
            [Display(Name = "Confirm Account Number")]
            public string ConfirmAccountNumber { get; set; }

            [Required]
            [Display(Name = "Account Type")]
            public string AccountType { get; set; }

            public MessageModel Payment { get; set; }
        }
        public class PersonalizeModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ImageUrl { get; set; }
        }
        public class MessageModel
        {
            public string Id { get; set; }
            public string RecipientUri { get; set; }
            public string MessageType { get; set; }
            public string Sender { get; set; }
            public string MobileNumber { get; set; }
            public Double Amount { get; set; }
            public string Comments { get; set; }
            public string SenderImageUrl { get; set; }
        }
        public class SetupPinSwipeModel
        {
            public string PinCode { get; set; }
        }
        public class ConfirmPinSwipeModel
        {
            public string PinCode { get; set; }
        }
        public class SecurityQuestionModel
        {
            public int SecurityQuestionId { get; set; }
            public string SecurityQuestionAnswer { get; set; }
            public List<SecurityQuestionModels.SecurityQuestionModel> SecurityQuestions { get; set; }
        }
}