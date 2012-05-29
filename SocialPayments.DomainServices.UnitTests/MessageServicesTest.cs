using SocialPayments.DomainServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.Domain;
using SocialPayments.DomainServices.UnitTests.Fakes;

namespace SocialPayments.DomainServices.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for MessageServicesTest and is intended
    ///to contain all MessageServicesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MessageServicesTest
    {
        private IDbContext _ctx = new FakeDbContext();
        private UserService _userService;
        private MessageServices _messageService;
        private PaymentAccountService _paymentAccountService;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void WhenPaymentMessageReceivedWithRecipientUriUnRegisteredMobileNumberMessageCountIs1()
        {
            _userService = new UserService(_ctx);
            _messageService = new MessageServices(_ctx);
            _paymentAccountService = new PaymentAccountService(_ctx);

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = "2578";
            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            sender.SecurityPin = securityPin;
            _userService.UpdateUser(sender);

            var senderAccount = _paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "James G Rhodes", "053000219",
                "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderAccount);

            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "804-350-9542", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", securityPin);

            Assert.AreEqual(_ctx.Messages.Count(), 1);

        }
        [TestMethod()]
        public void WhenPaymentMessageReceivedWithRecipientUriUnRegistreredEmailAddressMessageCountIs1()
        {
            _userService = new UserService(_ctx);
            _messageService = new MessageServices(_ctx);
            _paymentAccountService = new PaymentAccountService(_ctx);

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = "2589";

            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            sender.SecurityPin = securityPin;
            _userService.UpdateUser(sender);

            var senderAccount = _paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "James G Rhodes", "053000219",
                "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderAccount);

            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "james@paidthx.com", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", securityPin);

            Assert.AreEqual(_ctx.Messages.Count(), 1);

        }

        [TestMethod()]
        public void WhenPaymentMessageReceivedWithRecipientUriRegisteredMeCodeMessageCountIs1()
        {
            _userService = new UserService(_ctx);
            _messageService = new MessageServices(_ctx);
            _paymentAccountService = new PaymentAccountService(_ctx);

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = "2589";

            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            var senderAccount = _paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "James G Rhodes", "053000219",
                "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderAccount);

            var recipient = _userService.AddUser(application.ApiKey, "james@paidthx.com",
                "james123", "james@paidthx.com", "");

            var recipientAccount = _paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "James G Rhodes", "053000219",
                "1234123412", "Checking");

            recipient.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            recipient.PaymentAccounts.Add(recipientAccount);

            var meCodeValue = "$therealjamesrhodes";

            var meCode = _ctx.MECodes.Add(
                new MECode()
                {
                    ApprovedDate = System.DateTime.Now,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    IsActive = true,
                    IsApproved = true,
                    LastUpdatedDate = System.DateTime.Now,
                    MeCode = meCodeValue,
                    UserId = recipient.UserId
                });

            sender.SecurityPin = securityPin;
            _userService.UpdateUser(sender);

            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "$therealjamesrhodes", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", securityPin);

            Assert.AreEqual(_ctx.Messages.Count(), 1);

        }
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException))]
        public void WhenPaymentMessageReceivedWithRecipientUriUnKnownMeCodeArgumentExceptionOccurs()
        {
            _userService = new UserService(_ctx);
            _messageService = new MessageServices(_ctx);
            _paymentAccountService = new PaymentAccountService(_ctx);

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = "2589";

            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            sender.SecurityPin = securityPin;
            _userService.UpdateUser(sender);

            var senderAccount = _paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "James G Rhodes", "053000219",
                "1234123412", "Checking");
             
            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderAccount);

            var recipient = _userService.AddUser(application.ApiKey, "james@paidthx.com",
                "james123", "james@paidthx.com", "");

            var recipientAccount = _paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "James G Rhodes", "053000219",
                "1234123412", "Checking");

            recipient.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            recipient.PaymentAccounts.Add(recipientAccount);

            var meCodeValue = "$therealjamesrhodes";

            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, meCodeValue, senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", securityPin);

        }
    }
}
