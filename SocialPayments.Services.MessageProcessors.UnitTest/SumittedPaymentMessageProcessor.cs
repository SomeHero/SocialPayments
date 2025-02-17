﻿using System;
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
                EmailAddress = "chris@pdthx.me",
                Limit = 100,
                MobileNumber = "7082504915",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "Chris Magee",
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
                RecipientUri = "7082504915",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri ="7082504915",
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
                FirstName = "Your",
                LastName = "Mommy",
                EmailAddress = "chris@pdthx.me",
                Limit = 100,
                MobileNumber = "7082504915",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            var recipient = _ctx.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = "chris@pdthx.me",
                Limit = 100,
                MobileNumber = "7082504915",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "Chris Magee",
                RoutingNumber = "053000219",
            };

            var recipientAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1010202030",
                AccountType = Domain.PaymentAccountType.Savings,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "Chris Magee",
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
                MessageType = Domain.MessageType.PaymentRequest,
                RecipientUri = "7082504915",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "7082504915",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });
            

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            var message2 = _ctx.Messages.Add(new Domain.Message()
            {
                Amount = 1,
                Application = application,
                ApiKey = apiKey,
                Comments = "Test Payment",
                CreateDate = System.DateTime.Now,
                Id = messageId,
                MessageStatus = Domain.MessageStatus.Pending,
                MessageType = Domain.MessageType.Payment,
                RecipientUri = "7082504915",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "7082504915",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });

            processor.Process(message2);

            Assert.AreEqual(4, _ctx.Transactions.Count()); // Two Transactions are Created Here
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
                EmailAddress = "chris@pdthx.me",
                Limit = 100,
                MobileNumber = "7082504915",
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
                NameOnAccount = "Chris Magee",
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
                SenderUri = "7082504915",
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
                EmailAddress = "chris@pdthx.me",
                Limit = 100,
                MobileNumber = "7082504915",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            var recipient = _ctx.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = "james@paidthx.com",
                Limit = 100,
                MobileNumber = "8043879693",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            _ctx.SaveChanges();

            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "Chris Magee",
                RoutingNumber = "053000219",
            };

            var recipientAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1010202030",
                AccountType = Domain.PaymentAccountType.Savings,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = senderAccountId,
                NameOnAccount = "Chris Magee",
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
                RecipientUri = "james@paidthx.com",
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = "7082504915",
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
                EmailAddress = "chris@pdthx.me",
                Limit = 100,
                MobileNumber = "7082504915",
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Verified,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4",
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
                SenderUri = "7082504915",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(2, _ctx.Transactions.Count());
        }

        [TestMethod]
        public void sendPaymentToAndroidPhone()
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
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4",
                RegistrationId = "APA91bFvTMvUOIwR2neHvWdENYpmydo4eyLayM9ABl17Tj0WlR7n5u3O7Lf0PX-O3V5FbADew9c2dlpE9FmBXbdfGSSA45PTA_fjJjxJxM0Ld_ps7qE52DIM8pSP-yAR8sZK3Hv5mDCNPyxQ1YePpARP5ZAl1dUeFQ"
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
                SenderUri = "7574691582",
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>()
            });

            _ctx.SaveChanges();

            SubmittedPaymentMessageProcessor processor = new SubmittedPaymentMessageProcessor(_ctx);

            processor.Process(message);

            Assert.AreEqual(1, _ctx.Transactions.Count());
            
        }
    }

    
}
