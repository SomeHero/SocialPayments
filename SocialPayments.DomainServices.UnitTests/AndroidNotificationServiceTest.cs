using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DomainServices.UnitTests.Fakes;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.DomainServices.UnitTests
{
    [TestClass]
    public class AndroidNotificationServiceTest
    {

        private TestContext testContextInstance;
        private IDbContext _ctx;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        [TestCategory("Android Notification")]
        public void AndroidNotificationTests()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();
            string registrationId = "APA91bFvTMvUOIwR2neHvWdENYpmydo4eyLayM9ABl17Tj0WlR7n5u3O7Lf0PX-O3V5FbADew9c2dlpE9FmBXbdfGSSA45PTA_fjJjxJxM0Ld_ps7qE52DIM8pSP-yAR8sZK3Hv5mDCNPyxQ1YePpARP5ZAl1dUeFQ";

            _ctx = new FakeDbContext();

            var application = _ctx.Applications.Add(new Domain.Application()
            {
                ApiKey = apiKey,
                ApplicationName = "Test App",
                IsActive = true,
                Url = "http:\\test.paidthx.com"
            });

            var sender = _ctx.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = "edward@pdthx.me",
                Limit = 100,
                MobileNumber = "4439777232",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "d519726671dbe26a",
                RegistrationId = registrationId
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "Edward Mitchell",
                RoutingNumber = "053000219",
            };

            sender.PaymentAccounts.Add(senderAccount);

            _ctx.SaveChanges();

            var message = _ctx.Messages.Add(new Domain.Message()
            {
                Amount = 1,
                Application = application,
                ApiKey = apiKey,
                Comments = "Test Payment",
                CreateDate = System.DateTime.Now,
                Id = messageId,
                MessageStatus = Domain.MessageStatus.Pending,
                MessageType = Domain.MessageType.Payment,
                RecipientUri = "4439777232",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri ="7574691582",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>()
            });

            _ctx.SaveChanges();

            string auth_token = AndroidNotificationService.getToken("android.paidthx@gmail.com", "pdthx123");
            Assert.AreEqual(201, AndroidNotificationService.sendAndroidPushNotification(auth_token, senderId.ToString(), registrationId, message));
        }
    }
}
