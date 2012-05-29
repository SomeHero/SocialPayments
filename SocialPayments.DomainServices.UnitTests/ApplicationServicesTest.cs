using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;

namespace SocialPayments.DomainServices.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ApplicationServicesTest
    {
        private IDbContext _ctx = new FakeDbContext();
        private ApplicationService _applicationService;

        public ApplicationServicesTest()
        {
            _applicationService = new ApplicationService(_ctx);

            _ctx.Applications.Add(new Domain.Application()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                ApplicationName = "Test Application",
                IsActive = true,
                Url = "http://www.test.com"
            });

            _ctx.SaveChanges();
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
        public void WhenAddingApplicationCollectionHasNewItem()
        {
            int count = _ctx.Applications.Count();

            string name = "New Application";
            string url = "http://www.newurl.com";
            bool isActive = true;

            _applicationService.AddApplication(name, url, isActive);

            Assert.AreEqual(_ctx.Applications.Count(), count + 1);
        }
        [TestMethod]
        public void WhenUpdatingNameApplicationNameIsChanged()
        {
            string apiKey = "bda11d91-7ade-4da1-855d-24adfe39d174";
            string applicationName = "Application Name";
            string updatedApplicationName = "Update Application Name";
            bool isActive = true;
            string url = "http://www.test.com";

            var application = _applicationService.AddApplication(applicationName, url, isActive);

            application.ApplicationName = updatedApplicationName;
            _applicationService.UpdateApplication(application);

            Assert.AreEqual(updatedApplicationName, application.ApplicationName);

        }
    }
}
