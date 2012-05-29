using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices;
using SocialPayments.Services.PaymentAccounts;
using SocialPayments.DomainServices.UnitTests.Fakes;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.Service.PaymentAccountsProcessors.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        private IDbContext _ctx;

        public UnitTest1()
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
        public void WhenNewAccountIsAddedPaymentVerificationIsCreated()
        {
            _ctx = new FakeDbContext();

            UserService userService = new UserService(_ctx);
            IEmailService emailService = null;
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            SubmittedPaymentAccountProcessor processor = new SubmittedPaymentAccountProcessor(_ctx, emailService);

            Guid userId = Guid.NewGuid();

            var user = userService.AddUser(userId, "jrhodes621", "james123", "jrhodes621@gmail.com", "1234");
            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James G Rhodes", "053000219", "1234123412",
                "Checking");

            processor.Process(paymentAccount);

            var paymentAccountVerification = _ctx.PaymentAccountVerifications.ElementAt(0);

            Assert.AreEqual(paymentAccountVerification.PaymentAccountId, paymentAccount.Id);


        }
        [TestMethod]
        public void WhenNewAccountIsAddedPaymentVerificationDepositAmountsAreDifferent()
        {
            _ctx = new FakeDbContext();

            UserService userService = new UserService(_ctx);
            IEmailService emailService = null;
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            SubmittedPaymentAccountProcessor processor = new SubmittedPaymentAccountProcessor(_ctx, emailService);

            Guid userId = Guid.NewGuid();

            var user = userService.AddUser(userId, "jrhodes621", "james123", "jrhodes621@gmail.com", "1234");
            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James G Rhodes", "053000219", "1234123412",
                "Checking");

            processor.Process(paymentAccount);

            var paymentAccountVerification = _ctx.PaymentAccountVerifications.ElementAt(0);

            Assert.AreNotEqual(paymentAccountVerification.DepositAmount1, paymentAccountVerification.DepositAmount2);
        }
        [TestMethod]
        public void WhenNewAccountIsAddedPaymentVerificationWithdrawalAmountEqualTheSumOfDeposits()
        {
            _ctx = new FakeDbContext();

            UserService userService = new UserService(_ctx);
            IEmailService emailService = null;
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            SubmittedPaymentAccountProcessor processor = new SubmittedPaymentAccountProcessor(_ctx, emailService);

            Guid userId = Guid.NewGuid();

            var user = userService.AddUser(userId, "jrhodes621", "james123", "jrhodes621@gmail.com", "1234");
            var paymentAccount = paymentAccountService.AddPaymentAccount(userId.ToString(), "James G Rhodes", "053000219", "1234123412",
                "Checking");

            processor.Process(paymentAccount);

            var paymentAccountVerification = _ctx.PaymentAccountVerifications.ElementAt(0);

            Assert.AreEqual(paymentAccountVerification.WithdrawalAmount, paymentAccountVerification.DepositAmount1 + paymentAccountVerification.DepositAmount2);
        }
    }
}
