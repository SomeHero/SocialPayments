using SocialPayments.DomainServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SocialPayments.DomainServices.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for FormattingServicesTest and is intended
    ///to contain all FormattingServicesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FormattingServicesTest
    {


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
        ///A test for FormatMobileNumber
        ///</summary>
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensPhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "804-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensAndParensPhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "(804)-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensAndParensAndLeading1PhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "1-(804)-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensAndParensAndLeading1AndSpacePhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "1 (804)-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensAndParensAndLeading1NoSpacePhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "1(804)-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensAndParensAndPlusAndLeading1AndNoSpacePhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "+1(804)-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithHyphensAndParensAndPlusAndLeading1AndSpacePhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "+1 (804)-387-9693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [TestCategory("Mobile Number Formatting")]
        public void WhenPhoneNumberWithAllDigitsPhoneNumberIsFormatted()
        {
            FormattingServices target = new FormattingServices();
            string mobileNumber = "8043879693";
            string expected = "(804) 387-9693";
            string actual = target.FormatMobileNumber(mobileNumber);

            Assert.AreEqual(expected, actual);
        }
    }
}
