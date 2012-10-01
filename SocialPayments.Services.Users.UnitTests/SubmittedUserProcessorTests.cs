using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.Services.Users.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SubmittedUserProcessorTests
    {
        private IDbContext _ctx;
        private UserProcessors.Interfaces.IUserProcessor _processor;
        private IEmailService _emailService;
        private ISMSService _smsService;

        public SubmittedUserProcessorTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void WhenANewUserRegistersElasticEmailIsSent()
        {
            _ctx = new FakeDbContext();
            _emailService = new FakeEmailService();
            _smsService = new FakeSMSService();
            _processor = new UserProcessors.SubmittedUserProcessor(_ctx, _emailService, _smsService);
            
            var application = _ctx.Applications.Add(new Domain.Application() {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "test",
                IsActive = true,
                Url = "http://www.paidthx.com"
            });

            var user = _ctx.Users.Add(new Domain.User()
            {
                ApiKey  = application.ApiKey,
                ConfirmationToken = "1234",
                CreateDate = System.DateTime.Now,
                EmailAddress = "ryan@paidthx.com",
                IsConfirmed = false,
                MobileNumber = "804-387-9693",
                IsLockedOut = false,
                Limit = 100,
                SecurityPin = "1234",
                UserId = Guid.NewGuid(),
                UserName = "ryan@paidthx.com",
                UserStatus = Domain.UserStatus.Submitted
            });

            _processor.Process(user);

            Assert.IsTrue(((FakeEmailService)_emailService).WasCalled);
        }

        [TestMethod]
        public void WhenANewUserRegistersMicroPaymentsAreCreated()
        {

        }


    }
}
