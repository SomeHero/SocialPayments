using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain.Interfaces
{
    public interface IUser
    {
        Guid UserId { get; set; }
        Guid ApiKey { get; set; }
        IApplication Application { get; set; }
        string UserName { get; set; }
        string EmailAddress { get; set; }
        string Password { get; set; }
        bool IsConfirmed { get; set; }
        int PasswordFailuresSinceLastSuccess { get; set; }
        Nullable<DateTime> LastPasswordFailureDate { get; set; }
        string ConfirmationToken { get; set; }
        Nullable<DateTime> CreateDate { get; set; }
        Nullable<DateTime> PasswordChangedDate { get; set; }
        string MobileVerificationCode1 { get; set; }
        string MobileVerificationCode2 { get; set; }
        string PasswordVerificationToken { get; set; }
        Nullable<DateTime> PasswordVerificationTokenExpirationDate { get; set; }
        Collection<IRole> Roles { get; set; }
        string MobileNumber { get; set; }
        string SecurityPin { get; set; }
        bool IsLockedOut { get; set; }
        string TimeZone { get; set; }
        string Culture { get; set; }
        Collection<IUserAttributeValue> UserAttributes { get; set; }
        int UserStatusValue { get; set; }
        UserStatus UserStatus { get; set; }
        int RegistrationMethodValue { get; set; }
        UserRegistrationMethod RegistrationMethod { get; set; }
        DateTime LastLoggedIn { get; set; }
        double Limit { get; set; }
        bool SetupSecurityPin { get; set; }
        bool SetupPassword { get; set; }
        Collection<IPaymentAccount> PaymentAccounts { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string State { get; set; }
        string Zip { get; set; }
        string SenderName { get; set; }
        IFBUser FacebookUser { get; set; }

        Collection<IMessage> Messages { get; set; }
        string DeviceToken { get; set; }
    }
}
