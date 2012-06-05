using SocialPayments.DomainServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.Domain;
using SocialPayments.DomainServices.UnitTests.Fakes;
using NLog;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.Services.MessageProcessors.UnitTest;
using SocialPayments.DomainServices.CustomExceptions;

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
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private UserService _userService;
        private MessageServices _messageService;
        private PaymentAccountService _paymentAccountService;
        private TransactionBatchService _transactionBatchService;
        private SecurityService _securityService;
        private IAmazonNotificationService _amazonNotificationService;

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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
           
        }
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
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

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
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

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
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

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
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

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
        [TestMethod]
        public void WhenPaymentMessageThatIsInOpenBatchIsCancelledThenMessageStatusIsCancelled()
        {
            _userService = new UserService(_ctx);
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            sender.SecurityPin = _securityService.Encrypt(securityPin);
            sender.SetupSecurityPin = true;

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


            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, recipient.EmailAddress,
                senderAccount.Id.ToString(), 1.00, "Test Payment", "Payment", securityPin);

            message.Transactions = new System.Collections.ObjectModel.Collection<Transaction>();

            _ctx.SaveChanges();

            var transactions =  _transactionBatchService.BatchTransactions(message);

            foreach (var transaction in transactions)
            {
                message.Transactions.Add(transaction);
            }

            _messageService.CancelMessage(message.Id.ToString());

            Assert.AreEqual(MessageStatus.Cancelled, message.MessageStatus);
            Assert.IsTrue(((FakeAmazonNotificationService)_amazonNotificationService).WasCalled);

        }
        [TestMethod]
        public void WhenPaymentRequestIsRejectedThenMessageStatusIsUpdatedToRequestCancelled()
        {
            _userService = new UserService(_ctx);
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            sender.SecurityPin = _securityService.Encrypt(securityPin);
            sender.SetupSecurityPin = true;

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


            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, recipient.EmailAddress,
                senderAccount.Id.ToString(), 1.00, "Test Payment", "PaymentRequest", securityPin);

            message.Transactions = new System.Collections.ObjectModel.Collection<Transaction>();

            _ctx.SaveChanges();

            _messageService.RejectPaymentRequest(message.Id.ToString());

            Assert.AreEqual(MessageStatus.RequestRejected, message.MessageStatus);
            Assert.IsTrue(((FakeAmazonNotificationService)_amazonNotificationService).WasCalled);
        }
        [TestMethod]
        public void WhenPaymentRequestIsAcceptedThenMessageStatusIsUpdatedToRequestAccepted()
        {
            _userService = new UserService(_ctx);
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            _paymentAccountService = new PaymentAccountService(_ctx);
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _securityService = new SecurityService();

            var application = _ctx.Applications.Add(new Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test",
                IsActive = true,
                Url = "http://www.test.com"
            });

            var securityPin = _securityService.Encrypt("2589");

            var sender = _userService.AddUser(application.ApiKey, "jrhodes@paidthx.com",
                "james123", "jrhodes2705@gmail.com", "");

            sender.MobileNumber = "804-387-9693";
            _userService.UpdateUser(sender);

            sender.SecurityPin = _securityService.Encrypt(securityPin);
            sender.SetupSecurityPin = true;

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


            var message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, recipient.EmailAddress,
                senderAccount.Id.ToString(), 1.00, "Test Payment", "Payment", securityPin);

            message.Recipient = recipient;

            message.Transactions = new System.Collections.ObjectModel.Collection<Transaction>();

            _ctx.SaveChanges();

            _messageService.AcceptPaymentRequest(message.Id.ToString());

            var transactionBatch = _transactionBatchService.GetOpenBatch();

            Assert.AreEqual(MessageStatus.RequestAcceptedPending, message.MessageStatus);
            Assert.IsTrue(((FakeAmazonNotificationService)_amazonNotificationService).WasCalled);
        }
        [TestMethod]
        public void WhenSubmittingAPaymentWithInvalidPinCodeThenNumberOfPinCodeFailuesIncrements()
        {
            _securityService = new SecurityService();
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);
            
            var senderEmail = "james@paidthx.com";
            var senderMobileNumber = "8043879693";

            var application =  Mother.CreateApplication(_ctx);
            var sender = Mother.CreateSender(_ctx, application, senderEmail, senderMobileNumber);
            var senderAccount = Mother.CreatePaymentAccount(_ctx, sender);

            sender.SecurityPin = _securityService.Encrypt("2589");

            Domain.Message message = null;
            int pinCodeFailures = 0;

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            Assert.AreEqual(pinCodeFailures, sender.PinCodeFailuresSinceLastSuccess);

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            Assert.AreEqual(pinCodeFailures, sender.PinCodeFailuresSinceLastSuccess);

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            Assert.AreEqual(pinCodeFailures, sender.PinCodeFailuresSinceLastSuccess);

            //Assert.IsNotNull(sender.PinCodeLockOutResetTimeout);

        }

        [TestMethod]
        public void WhenSubmittingAPaymentWithInvalidPinCode3TimesThenPinCodeLockOutResetTimeIsNotNull()
        {
            _securityService = new SecurityService();
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);

            var senderEmail = "james@paidthx.com";
            var senderMobileNumber = "8043879693";

            var application = Mother.CreateApplication(_ctx);
            var sender = Mother.CreateSender(_ctx, application, senderEmail, senderMobileNumber);
            var senderAccount = Mother.CreatePaymentAccount(_ctx, sender);

            sender.SecurityPin = _securityService.Encrypt("2589");

            Domain.Message message = null;
            int pinCodeFailures = 0;

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (AccountLockedPinCodeFailures ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            Assert.IsNotNull(sender.PinCodeLockOutResetTimeout);

        }
        [TestMethod]
        [ExpectedException(typeof(AccountLockedPinCodeFailures))]
        public void WhenSubmittingAPaymentWithInvalidPinCode3TimesAndThenSubmittedAgainThenAccountLockedOutExceptionOccurs()
        {
            _securityService = new SecurityService();
            _amazonNotificationService = new FakeAmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);

            var senderEmail = "james@paidthx.com";
            var senderMobileNumber = "8043879693";

            var application = Mother.CreateApplication(_ctx);
            var sender = Mother.CreateSender(_ctx, application, senderEmail, senderMobileNumber);
            var senderAccount = Mother.CreatePaymentAccount(_ctx, sender);

            sender.SecurityPin = _securityService.Encrypt("2589");

            Domain.Message message = null;
            int pinCodeFailures = 0;

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (Exception ex)
            {
                //ignore ex
            }

            try
            {
                message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "1111");
            }
            catch (AccountLockedPinCodeFailures ex)
            {
                //ignore ex
            }

            pinCodeFailures += 1;

            message = _messageService.AddMessage(application.ApiKey.ToString(), sender.MobileNumber, "8043550001", senderAccount.Id.ToString(),
                10.00, "Test Payment", "Payment", "2589");

        }
    }
}
