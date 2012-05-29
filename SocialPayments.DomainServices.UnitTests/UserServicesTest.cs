using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;
using NLog;

namespace SocialPayments.DomainServices.UnitTests
{
    [TestClass]
    public class UserServicesTest
    {
        private IDbContext _ctx = new FakeDbContext();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private UserService _userService;

        [TestMethod]
        public void WhenUserRegistersUserIsAddedToUser()
        {
            _userService = new UserService(_ctx);

            var username = "jrhodes621@gmail.com";
            var password = "james123";
            var user = _userService.AddUser(Guid.NewGuid(), username, password, username, "1234");

            Assert.AreEqual(_ctx.Users.ElementAt(0), user);
            Assert.AreEqual(_ctx.Users.ElementAt(0).UserName, username);
            Assert.AreEqual(_ctx.Users.ElementAt(0).EmailAddress, username);

        }
        [TestMethod]
        public void WhenAddingFormattedMobileNumberToUserMobileNumberIsUnFormatted()
        {

        }
        [TestMethod]
        public void WhenFindingUserByEmailAddressUserWithEmailAddressIsFound()
        {
            _userService = new UserService(_ctx);
            
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");

            var foundUser = _userService.FindUserByEmailAddress(emailAddress);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenFindingUserByFormattedMobileNumberUserWithMobileNumberIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber= "(804) 387-9693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            var foundUser = _userService.FindUserByMobileNumber(mobileNumber);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenFindingUserByUnFormattedMobilieNumberUserWithMobileNumberIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "8043879693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            var foundUser = _userService.FindUserByMobileNumber(mobileNumber);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenFindingUserByPhoneNumberWithLeading1UserIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "18043879693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            var foundUser = _userService.FindUserByMobileNumber(mobileNumber);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenFindingUserByPhoneNumberWithLeadingPlus1UserIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "+18043879693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            var foundUser = _userService.FindUserByMobileNumber(mobileNumber);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenFindingUserByIdUserWithIdIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "8043879693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            var foundUser = _userService.GetUserById(user.UserId.ToString());

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenUserSetsUpSecurityPinSecurityPinIsSetup()
        {

        }
        [TestMethod]
        public void WhenValidatingUserByEmailAddressValidUserIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "8043879693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            Domain.User foundUser;
            _userService.ValidateUser("jrhodes621@gmail.com", "james123", out foundUser);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenValidatingUserByUnformattedValidUserIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "(804) 387-9693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            Domain.User foundUser;
            _userService.ValidateUser("8043879693", "james123", out foundUser);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenValidatingUserWithUnformattedPhoneNumberWithHyphensValidUserIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "804-387-9693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            Domain.User foundUser;
            _userService.ValidateUser("8043879693", "james123", out foundUser);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenValidatingUserFormattedValidUserIsFound()
        {
            _userService = new UserService(_ctx);

            var mobileNumber = "8043879693";
            var emailAddress = "jrhodes621@gmail.com";

            var user = _userService.AddUser(Guid.NewGuid(), emailAddress, "james123", emailAddress, "");
            user.MobileNumber = mobileNumber;

            _userService.UpdateUser(user);

            Domain.User foundUser;
            _userService.ValidateUser("8043879693", "james123", out foundUser);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void RegisteredUserWithFacebookAccountCanSignIn()
        {
            var apiKey = Guid.NewGuid();
            var fbAccountId = "1234abcd";
            var emailAddress = "james@paidthx.com";
            var firstName = "James";
            var lastName = "Rhodes";
            
            var memberRole = _ctx.Roles.Add(new Domain.Role() {
                Description = "Member",
                RoleId = Guid.NewGuid(),
                RoleName = "Member"
            });

            _userService = new UserService(_ctx);

            var user = _ctx.Users.Add(new Domain.User() {
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                DeviceToken = "",
                EmailAddress = emailAddress,
                FacebookUser = new Domain.FBUser() {
                    FBUserID = fbAccountId,
                    Id = Guid.NewGuid(),
                },
                FirstName = firstName,
                LastName = lastName,
                Limit = 100,
                Roles = new System.Collections.ObjectModel.Collection<Domain.Role>() {
                    memberRole
                },
                RegistrationMethod = Domain.UserRegistrationMethod.MobilePhone,
                SenderName = firstName + " " + lastName,
                SetupPassword = true,
                SetupSecurityPin = true,
                UserId = Guid.NewGuid(),
                UserName = "fb_" + fbAccountId,
                UserStatus = Domain.UserStatus.Pending
            });

            _ctx.SaveChanges();

           var fbUser =  _userService.SignInWithFacebook(apiKey, fbAccountId, emailAddress, firstName, lastName, "");

           Assert.AreEqual(fbUser, user);
        }
        [TestMethod]
        public void NewUserWithFacebookAccountCanSignIn()
        {
            var apiKey = Guid.NewGuid();
            var fbAccountId = "1234abcd";
            var emailAddress = "james@paidthx.com";
            var firstName = "James";
            var lastName = "Rhodes";

            var memberRole = _ctx.Roles.Add(new Domain.Role()
            {
                Description = "Member",
                RoleId = Guid.NewGuid(),
                RoleName = "Member"
            });

            _userService = new UserService(_ctx);

            var fbUser = _userService.SignInWithFacebook(apiKey, fbAccountId, emailAddress, firstName, lastName, "");

            Assert.AreEqual(fbUser.EmailAddress, emailAddress);
        }
        [TestMethod]
        public void WhenVerifyingAccountCorrectlyVerificationServiceReturnsTrue()
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var paymentAccountVerificationService = new DomainServices.PaymentAccountVerificationService(_ctx, _logger);
            var userId = Guid.NewGuid();

            double depositAmount1 = 0.34;
            double depositAmount2 = 0.15; ;

            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James Rhodes",
                "053000219", "1234123412", "Savings");

            paymentAccount.AccountStatus = Domain.AccountStatusType.PendingActivation;
            paymentAccountService.UpdatePaymentAccount(paymentAccount);

            _ctx.PaymentAccountVerifications.Add(new Domain.PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                EstimatedSettlementDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NumberOfFailures = 0,
                PaymentAccountId = paymentAccount.Id,
                Sent = System.DateTime.Now,
                Status = Domain.PaymentAccountVerificationStatus.Deliveried
            });

            _ctx.SaveChanges();

            var result = paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), depositAmount1, depositAmount2);

            Assert.AreEqual(true, result);
        }
        [TestMethod]
        public void WhenVerifyingAccountCorrectlyAccountStatusUpdatestoVerified()
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var paymentAccountVerificationService = new DomainServices.PaymentAccountVerificationService(_ctx, _logger);
            var userId = Guid.NewGuid();

            double depositAmount1 = 0.34;
            double depositAmount2 = 0.15;;

            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James Rhodes",
                "053000219", "1234123412", "Savings");

            paymentAccount.AccountStatus = Domain.AccountStatusType.PendingActivation;
            paymentAccountService.UpdatePaymentAccount(paymentAccount);

            _ctx.PaymentAccountVerifications.Add(new Domain.PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                EstimatedSettlementDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NumberOfFailures = 0,
                PaymentAccountId = paymentAccount.Id,
                Sent = System.DateTime.Now,
                Status = Domain.PaymentAccountVerificationStatus.Deliveried
            });

            _ctx.SaveChanges();

            var result = paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), depositAmount1, depositAmount2);

            Assert.AreEqual(paymentAccount.AccountStatus, Domain.AccountStatusType.Verified);
        }
        [TestMethod]
        public void WhenVerifyingAccountInCorrectlyVerificationServiceReturnsFalse()
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var paymentAccountVerificationService = new DomainServices.PaymentAccountVerificationService(_ctx, _logger);
            var userId = Guid.NewGuid();

            double depositAmount1 = 0.34;
            double depositAmount2 = 0.15;

            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James Rhodes",
                "053000219", "1234123412", "Savings");

            paymentAccount.AccountStatus = Domain.AccountStatusType.PendingActivation;
            paymentAccountService.UpdatePaymentAccount(paymentAccount);

            _ctx.PaymentAccountVerifications.Add(new Domain.PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                EstimatedSettlementDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NumberOfFailures = 0,
                PaymentAccountId = paymentAccount.Id,
                Sent = System.DateTime.Now,
                Status = Domain.PaymentAccountVerificationStatus.Deliveried
            });

            _ctx.SaveChanges();

            var result = paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), 0.99, 0.01);

            Assert.AreEqual(false, result);
        }
        [TestMethod]
        public void WhenVerifyingAccountInCorrectlyAccountStatusRemainsPendingActivation()
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var paymentAccountVerificationService = new DomainServices.PaymentAccountVerificationService(_ctx, _logger);
            var userId = Guid.NewGuid();

            double depositAmount1 = 0.34;
            double depositAmount2 = 0.15;

            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James Rhodes",
                "053000219", "1234123412", "Savings");

            paymentAccount.AccountStatus = Domain.AccountStatusType.PendingActivation;
            paymentAccountService.UpdatePaymentAccount(paymentAccount);

            _ctx.PaymentAccountVerifications.Add(new Domain.PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                EstimatedSettlementDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NumberOfFailures = 0,
                PaymentAccountId = paymentAccount.Id,
                Sent = System.DateTime.Now,
                Status = Domain.PaymentAccountVerificationStatus.Deliveried
            });

            _ctx.SaveChanges();

            var result = paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), 0.99, 0.01);

            Assert.AreEqual(paymentAccount.AccountStatus, Domain.AccountStatusType.PendingActivation);
        }
        [TestMethod]
        public void WhenVerifyingAccountInCorrectlyNumberOfFailuresIncrements()
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var paymentAccountVerificationService = new DomainServices.PaymentAccountVerificationService(_ctx, _logger);
            var userId = Guid.NewGuid();

            double depositAmount1 = 0.34;
            double depositAmount2 = 0.15;

            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James Rhodes",
                "053000219", "1234123412", "Savings");

            paymentAccount.AccountStatus = Domain.AccountStatusType.PendingActivation;
            paymentAccountService.UpdatePaymentAccount(paymentAccount);

            var paymentAccountVerification =  _ctx.PaymentAccountVerifications.Add(new Domain.PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                EstimatedSettlementDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NumberOfFailures = 0,
                PaymentAccountId = paymentAccount.Id,
                Sent = System.DateTime.Now,
                Status = Domain.PaymentAccountVerificationStatus.Deliveried
            });

            _ctx.SaveChanges();

            int numberOfFailuresBefore = paymentAccountVerification.NumberOfFailures;

            var result = paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), 0.99, 0.01);

            var paymentAccountVerificationAfter = _ctx.PaymentAccountVerifications.ElementAt(0);

            Assert.AreEqual(numberOfFailuresBefore + 1, paymentAccountVerificationAfter.NumberOfFailures);
        }
        [TestMethod]
        public void WhenVerifyingAccountInCorrectly3TimesAccountStatusIsChangedToNeedsReVerification()
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var paymentAccountVerificationService = new DomainServices.PaymentAccountVerificationService(_ctx, _logger);
            var userId = Guid.NewGuid();

            double depositAmount1 = 0.34;
            double depositAmount2 = 0.15;

            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James Rhodes",
                "053000219", "1234123412", "Savings");

            paymentAccount.AccountStatus = Domain.AccountStatusType.PendingActivation;
            paymentAccountService.UpdatePaymentAccount(paymentAccount);

            var paymentAccountVerification = _ctx.PaymentAccountVerifications.Add(new Domain.PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                EstimatedSettlementDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NumberOfFailures = 0,
                PaymentAccountId = paymentAccount.Id,
                Sent = System.DateTime.Now,
                Status = Domain.PaymentAccountVerificationStatus.Deliveried
            });

            _ctx.SaveChanges();

            int numberOfFailuresBefore = paymentAccountVerification.NumberOfFailures;

            paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), 0.99, 0.01);
            paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), 0.99, 0.01);
            paymentAccountVerificationService.VerifyAccount(userId.ToString(), paymentAccount.Id.ToString(), 0.99, 0.01);

            Assert.AreEqual(paymentAccount.AccountStatus, Domain.AccountStatusType.NeedsReVerification);
        }
    }
}
