using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

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
        [MaxLength(250)]
        [DataType(DataType.EmailAddress)]
        public virtual string EmailAddress { get; set; }
        [Required()]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public virtual string Password { get; set; }
        public virtual bool IsConfirmed { get; set; }
        public virtual int PasswordFailuresSinceLastSuccess { get; set; }
        public virtual int PinCodeFailuresSinceLastSuccess { get; set; }
        public virtual DateTime? PinCodeLockOutResetTimeout { get; set; }
        public virtual Nullable<DateTime> LastPasswordFailureDate { get; set; }
        [MaxLength(100)]
        public virtual string ConfirmationToken { get; set; }
        public virtual Nullable<DateTime> CreateDate { get; set; }
        public virtual Nullable<DateTime> PasswordChangedDate { get; set; }
        [MaxLength(10)]
        public virtual string MobileVerificationCode1 { get; set; }
        [MaxLength(10)]
        public virtual string MobileVerificationCode2 { get; set; }
        [MaxLength(100)]
        public virtual string PasswordVerificationToken { get; set; }
        public virtual Nullable<DateTime> PasswordVerificationTokenExpirationDate { get; set; }

        public virtual Collection<Role> Roles { get; set; }
        [MaxLength(50)]
        public virtual string MobileNumber { get; set; }
        public virtual string SecurityPin { get; set; }
        public virtual bool IsLockedOut { get; set; }
        [MaxLength(100)]
        public virtual string TimeZone { get; set; }
        [MaxLength(100)]
        public virtual string Culture { get; set; }
        public virtual Collection<UserAttributeValue> UserAttributes { get; set; }
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
        public virtual Collection<PaymentAccount> PaymentAccounts { get; set; }

        [MaxLength(255)]
        public string OrganizationName { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        [MaxLength(100)]
        public string Address { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [MaxLength(10)]
        public string Zip { get; set; }
        [MaxLength(255)]
        public string SenderName { get; set; }
        [Column(name:"FBUserId")]
        public virtual FBUser FacebookUser { get; set; }

        public virtual Collection<Message> Messages { get; set; }
        [MaxLength(100)]
        public string DeviceToken { get; set; }
        public string RegistrationId { get; set; }

        public virtual Collection<MECode> MECodes { get; set; }
        [MaxLength(255)]
        public string ImageUrl { get; set; }

        // Security Question
        public virtual SecurityQuestion SecurityQuestion { get; set; }
        public virtual int? SecurityQuestionID { get; set; }
        [MaxLength(50)]
        public virtual string SecurityQuestionAnswer { get; set; }

        //Preferred Accounts
        public virtual Guid? PreferredSendAccountId { get; set; }
        public virtual PaymentAccount PreferredSendAccount { get; set; }

        public virtual Guid? PreferredReceiveAccountId { get; set; }
        public virtual PaymentAccount PreferredReceiveAccount { get; set; }

        public int UserTypeId { get; set; }
        [ForeignKey("UserTypeId")]
        public virtual UserType UserType
        {
            get { return (UserType)UserType; }
            set { UserTypeId = (int)value; }
        }

        public DateTime? LastViewedPaystream { get; set; }

        public virtual Collection<UserPayPoint> PayPoints { get; set; }
        public virtual Collection<UserNotification> NotificationConfiguration { get; set; }

        public virtual Collection<UserConfiguration> UserConfigurations { get; set; }
        public virtual Collection<UserSocialNetwork> UserSocialNetworks { get; set; }

        public virtual Merchant Merchant { get; set; }

        //Express Delivery
        [DefaultValue(0)]
        public double ExpressDeliveryFreeThreshold { get; set; }
        [DefaultValue(0.0275)]
        public double ExpressDeliveryFeePercentage { get; set; }
        [DefaultValue(true)]
        public bool CanExpress { get; set; }
    }

}