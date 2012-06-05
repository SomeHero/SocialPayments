using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for EmailLogServiceTest and is intended
    ///to contain all EmailLogServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EmailLogServiceTest
    {

        private IDbContext _ctx = new FakeDbContext();
        private EmailLogService _emailLogService;
        private ApplicationService _applicationService;

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

        /// <summary>
        ///A test for AddEmailLog
        ///</summary>
        [TestMethod()]
        public void WhenAddingToEmailLogEmalLogIsAdded()
        {
            _emailLogService = new EmailLogService(_ctx);
            _applicationService = new ApplicationService(_ctx);

            var application = _applicationService.AddApplication("Test", "http://www.test.com", true);

            string fromAddress = "james@paidthx.com";
            string toAddress = "jrhodes2705@paidthx.com";
            string subject = "Test Email";
            string body = "Welcome to PaidThx";

            Nullable<DateTime> sentDate = System.DateTime.Now;

            EmailLog expected = _emailLogService.AddEmailLog(application.ApiKey, fromAddress, toAddress, subject, body, sentDate);

            EmailLog actual = _ctx.EmailLog.ElementAt(0);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetEmailLog
        ///</summary>
        [TestMethod()]
        public void WhenGettingEmailLogEntryByIdEmailLogIsReturned()
        {
            _emailLogService = new EmailLogService(_ctx); // TODO: Initialize to an appropriate value
            _applicationService = new ApplicationService(_ctx);

            var application = _applicationService.AddApplication("Test", "http://www.test.com", true);
            var id = Guid.NewGuid();
            var expected = _emailLogService.AddEmailLog(id, "james@paidthx.com", "jrhodes2705@gmail.com", "Test Email",
                "Welcome to PaidThx", System.DateTime.Now);
            var actual = _emailLogService.GetEmailLog(id);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UpdateEmailLog
        ///</summary>
        [TestMethod()]
        public void WhenUpdatingEmailLogThenEmailLogIsUpdated()
        {
            _emailLogService = new EmailLogService(_ctx);
            var application = _applicationService.AddApplication("Test", "http://www.test.com", true);
            var id = Guid.NewGuid();

            string fromAddress = "james@paidthx.com";
            string toAddress = "thomas@paidthx.com";
            string subject = "Test Email";
            string body = "Welcome to PaidThx";
            Nullable<DateTime> sentDate = System.DateTime.Now;

            var emailLog = _emailLogService.AddEmailLog(application.ApiKey, fromAddress, toAddress, subject, body,
                sentDate);

            emailLog.EmailStatus = EmailStatus.Sent;
            _emailLogService.UpdateEmailLog(emailLog);

            EmailLog expected = emailLog;
            EmailLog actual = _ctx.EmailLog.ElementAt(0);

            Assert.AreEqual(expected, actual);
        }
    }
}
