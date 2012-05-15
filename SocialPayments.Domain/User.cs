using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class User
    {

        //Membership required
        [Key(), Required()]
        public virtual Guid UserId { get; set; }
        [Required()]
        public Guid ApiKey { get; set; }
        [ForeignKey("ApiKey")]
        public virtual Application Application { get; set; }
        [Required()]
        [MaxLength(100)]
        public virtual string UserName { get; set; }
        [Required()]
        [MaxLength(250)]
        [DataType(DataType.EmailAddress)]
        public virtual string EmailAddress { get; set; }
        [Required()]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public virtual string Password { get; set; }
        public virtual bool IsConfirmed { get; set; }
        public virtual int PasswordFailuresSinceLastSuccess { get; set; }
        public virtual Nullable<DateTime> LastPasswordFailureDate { get; set; }
        public virtual string ConfirmationToken { get; set; }
        public virtual Nullable<DateTime> CreateDate { get; set; }
        public virtual Nullable<DateTime> PasswordChangedDate { get; set; }
        public virtual string MobileVerificationCode1 { get; set; }
        public virtual string MobileVerificationCode2 { get; set; }
        public virtual string PasswordVerificationToken { get; set; }
        public virtual Nullable<DateTime> PasswordVerificationTokenExpirationDate { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
        [Required()]
        public virtual string MobileNumber { get; set; }
        public virtual string SecurityPin { get; set; }
        public virtual bool IsLockedOut { get; set; }
        public virtual string TimeZone { get; set; }
        public virtual string Culture { get; set; }
        public virtual List<UserAttributeValue> UserAttributes { get; set; }
        public int UserStatusValue { get; set; }
        public virtual UserStatus UserStatus 
        { 
            get { return (UserStatus)UserStatusValue; }
            set { UserStatusValue = (int)value; } 
        }
        public int RegistrationMethodValue { get; set; }
        public UserRegistrationMethod RegistrationMethod
        {
            get { return (UserRegistrationMethod)RegistrationMethodValue; }
            set { RegistrationMethodValue = (int)value; }
        }
        public virtual DateTime LastLoggedIn { get; set; }
        [DefaultValue(100)]
        public virtual double Limit { get; set; }
        [DefaultValue(false)]
        public virtual bool SetupSecurityPin { get; set; }
        [DefaultValue(false)]
        public virtual bool SetupPassword { get; set; }
        public virtual List<PaymentAccount> PaymentAccounts { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string SenderName { get; set; }
        [Column(name:"FBUserId")]
        public FBUser FacebookUser { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}