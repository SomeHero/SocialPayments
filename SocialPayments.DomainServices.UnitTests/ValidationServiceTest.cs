using SocialPayments.DomainServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NLog;

namespace SocialPayments.DomainServices.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for ValidationServiceTest and is intended
    ///to contain all ValidationServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ValidationServiceTest
    {


        private TestContext testContextInstance;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ValidationService _validationService = new ValidationService(_logger);

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

        [TestMethod]
        public void WhenValidatingTwoMobileNumbersFormattedTheSameAreNumbersEqualIsTrue()
        { 
            var mobileNumber = "8043879693";
            var mobileNumberToCompareTo = "8043879693";

            var results = _validationService.AreMobileNumbersEqual(mobileNumber, mobileNumberToCompareTo);

            Assert.AreEqual(true, results);
        }

        [TestMethod]
        public void WhenValidatingTwoMobileNumbersThatAreTheSameButFormattedDifferentlyAreNumbersEqualIsTrue()
        {
            var mobileNumber = "804-387-9693";
            var mobileNumberToCompareTo = "8043879693";

            var results = _validationService.AreMobileNumbersEqual(mobileNumber, mobileNumberToCompareTo);

            Assert.AreEqual(true, results);
        }
        [TestMethod]
        public void WhenValidatingTwoMobileNumbersThatAreNotTheSameAreNumbersEqualsIsFalse()
        {
            var mobileNumber = "8043879600";
            var mobileNumberToCompareTo = "8043879693";

            var results = _validationService.AreMobileNumbersEqual(mobileNumber, mobileNumberToCompareTo);

            Assert.AreEqual(false, results);
        }

        [TestMethod]
        public void WhenValidatingEmailAddressThatIsValidIsEmailAddressIsTrue()
        {
            var emailAddress = "jrhodes@gmail.com";

            var results = _validationService.IsEmailAddress(emailAddress);
            Assert.AreEqual(true, results);
        }
        [TestMethod]
        public void WhenValidatingEmailAddressThatIsInvalidIsEmailAddressIsFalse()
        {
            var emailAddress = "jrhodes";

            var results = _validationService.IsEmailAddress(emailAddress);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingMobileNumberIsEmailAddressIsFalse()
        {
            var emailAddress = "8043879693";

            var results = _validationService.IsEmailAddress(emailAddress);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingMECodeIsEmailAddressIsFalse()
        {

            var meCode = "$therealjamesrhodes";

            var results = _validationService.IsEmailAddress(meCode);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingFacebookAccountIsEmailAddressIsFalse()
        {
            var facebookAccount = "fb_8043879693";

            var results = _validationService.IsEmailAddress(facebookAccount);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingMECodeThatIsFormattedCorrectlyIsMECodeIsTrue()
        {
            var meCode = "$therealjamesrhodes";

            var results = _validationService.IsMECode(meCode);
            Assert.AreEqual(true, results);
        }
        [TestMethod]
        public void WhenValidatingEmailAddressIsMECodeIsFalse()
        {
            var emailAddress = "james@paidthx.com";

            var results = _validationService.IsMECode(emailAddress);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingMobileNumberIsMECoodeIsFale()
        {
            var mobileNumber = "8043879693";

            var results = _validationService.IsMECode(mobileNumber);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingFacebookAccountIsMeCodeIsFalse()
        {
            var facebookAccount = "fb_1234";

            var results = _validationService.IsMECode(facebookAccount);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingFacebookAccountThatIsFormattedCorrectlyIsFacebookAccountIsTrue()
        {
            var facebookAccount = "fb_12343";

            var results = _validationService.IsFacebookAccount(facebookAccount);
            Assert.AreEqual(true, results);
        }
        [TestMethod]
        public void WhenValidatingEmailAddressIsFacebookAccountIsFalse()
        {
            var emailAddress = "jrhodes@mail.com";

            var results = _validationService.IsFacebookAccount(emailAddress);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingMECodeIsFacebookAccountIsFalse()
        {
            var meCode = "$therealjamesrhodes";

            var results = _validationService.IsFacebookAccount(meCode);
            Assert.AreEqual(false, results);
        }
        [TestMethod]
        public void WhenValidatingMobileNumberIsFacebookAccountIsFalse()
        {
            var mobileNumber = "8043879693";

            var results = _validationService.IsFacebookAccount(mobileNumber);
            Assert.AreEqual(false, results);
        }
    }
}
