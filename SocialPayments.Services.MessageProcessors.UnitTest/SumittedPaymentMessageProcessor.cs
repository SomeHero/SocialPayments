using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;

namespace SocialPayments.Services.MessageProcessors.UnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SumittedPaymentMessageProcessor
    {
        private IDbContext _ctx;

        public SumittedPaymentMessageProcessor()
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
        [TestInitialize()]
        public void MyTestInitialize() { 
        }
        
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup() { 
        }
        #endregion

        [TestMethod]
        public void WhenPendingPaymentMessageWithMobileNumberRecipientUriProcessedWithUnknownRecipientOneTransactionIsCreated()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();

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
                EmailAddress = "jrhodes2705@gmail.com",
                Limit = 100,
                MobileNumber = "8043550001",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
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
                RecipientUri = "8043879693",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri ="8043879693",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>()
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(1, _ctx.Transactions.Count());
        }
        [TestMethod]
        public void WhenPendingPaymentMessageWithMobileNumberRecipientUriProcessedWithKnownRecipientTwoTransactionsAreCreated()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();

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
                EmailAddress = "james@paidthx.com",
                Limit = 100,
                MobileNumber = "8043550001",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            var recipient = _ctx.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = "jrhodes2705@gmail.com",
                Limit = 100,
                MobileNumber = "8043879693",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000219",
            };

            var recipientAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1010202030",
                AccountType = Domain.PaymentAccountType.Savings,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000211",
            };


            sender.PaymentAccounts.Add(senderAccount);
            recipient.PaymentAccounts.Add(recipientAccount);

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
                RecipientUri = "8043879693",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "8043879693",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(2, _ctx.Transactions.Count());
        }
        [TestMethod]
        public void WhenPendingPaymentMessageWithEmailAddressRecipientUriProcessedWithUnknownRecipientOneTransactionIsCreated()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();

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
                EmailAddress = "jrhodes2705@gmail.com",
                Limit = 100,
                MobileNumber = "8043550001",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000219",
            };

            sender.PaymentAccounts.Add(senderAccount);

            _ctx.SaveChanges();

            var message = _ctx.Messages.Add(new Domain.Message()
            {
                Amount = 4,
                Application = application,
                ApiKey = apiKey,
                Comments = "Test Payment",
                CreateDate = System.DateTime.Now,
                Id = messageId,
                MessageStatus = Domain.MessageStatus.Pending,
                MessageType = Domain.MessageType.Payment,
                RecipientUri = "james@paidthx.com",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "8043879693",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>()
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(1, _ctx.Transactions.Count());
        }
       
        [TestMethod]
        public void WhenPendingPaymentMessageWithEmailAddressRecipientUriProcessedWithKnownRecipientTwoTransactionsAreCreated()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();

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
                EmailAddress = "james@paidthx.com",
                Limit = 100,
                MobileNumber = "8043550001",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            var recipient = _ctx.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = "jrhodes2705@gmail.com",
                Limit = 100,
                MobileNumber = "8043879693",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000219",
            };

            var recipientAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1010202030",
                AccountType = Domain.PaymentAccountType.Savings,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000211",
            };


            sender.PaymentAccounts.Add(senderAccount);
            recipient.PaymentAccounts.Add(recipientAccount);

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
                RecipientUri = "jrhodes2705@gmail.com",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "8043879693",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(2, _ctx.Transactions.Count());
        }
        
        [TestMethod]
        public void WhenPendingPaymentMessageWithMECodeRecipientUriProcessedWithKnownRecipientTwoTransactionsAreCreated()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();

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
                EmailAddress = "james@paidthx.com",
                Limit = 100,
                MobileNumber = "8043550001",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified
            });

            var recipient = _ctx.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = "jrhodes2705@gmail.com",
                Limit = 100,
                MobileNumber = "8043879693",
                Password = "james123",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                MECodes = new System.Collections.ObjectModel.Collection<Domain.MECode>()
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000219",
            };

            var recipientAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1010202030",
                AccountType = Domain.PaymentAccountType.Savings,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "James Rhodes",
                RoutingNumber = "053000211",
            };


            sender.PaymentAccounts.Add(senderAccount);
            recipient.PaymentAccounts.Add(recipientAccount);

            var meCode = new Domain.MECode()
            {
                ApprovedDate = System.DateTime.Now,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                IsActive = true,
                IsApproved = true,
                LastUpdatedDate= System.DateTime.Now,
                MeCode = "$jrhodes",
                User = recipient
            };

            _ctx.MECodes.Add(meCode);
            recipient.MECodes.Add(meCode);

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
                RecipientUri = "$jrhodes",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "8043879693",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(2, _ctx.Transactions.Count());
        }
    }
}
