using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;

namespace SocialPayments.DomainServices.UnitTests
{
    [TestClass]
    public class UserServicesTest
    {
        private IDbContext _ctx = new FakeDbContext();
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
        public void WhenAddingMobileNumberToUserMobileNumberIsUnFormatted()
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
            user.MobileNumber = "8043879693";
            _ctx.SaveChanges();

            var foundUser = _userService.FindUserByMobileNumber(mobileNumber);

            Assert.AreEqual(user, foundUser);
        }
        [TestMethod]
        public void WhenFindingUserByUnFormattedMobilieNumberUserWithMobileNumberIsFound()
        {

        }
        [TestMethod]
        public void WhenFindingUserByIdUserWithIdIsFound()
        {

        }
        [TestMethod]
        public void WhenUserSetsUpSecurityPinSecurityPinIsSetup()
        {

        }
        [TestMethod]
        public void WhenValidatingUserValidUserIsFound()
        {

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
    }
}
