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

           var fbUser =  _userService.SignInWithFacebook(apiKey, fbAccountId, emailAddress, firstName, lastName);

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

            var fbUser = _userService.SignInWithFacebook(apiKey, fbAccountId, emailAddress, firstName, lastName);

            Assert.AreEqual(fbUser.EmailAddress, emailAddress);
        }
    }
}
