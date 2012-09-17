using SocialPayments.DomainServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.Domain;
using System.Collections.Generic;
using System.Linq.Expressions;
using SocialPayments.DomainServices.UnitTests.Fakes;
using SocialPayments.DomainServices.Interfaces;
using System.Collections.ObjectModel;

namespace SocialPayments.DomainServices.UnitTests
{


    /// <summary>
    ///This is a test class for TransactionBatchServiceTest and is intended
    ///to contain all TransactionBatchServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TransactionBatchServiceTest
    {

        private IDbContext _ctx = new FakeDbContext();
        private Logger _logger = LogManager.GetCurrentClassLogger();
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


        [TestMethod]
        public void WhenBatchingMessageWithOpenBatchWithWithdrawAndDepositTransactionThenTransactionsInBatchIs2()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            IAmazonNotificationService amazonNotificationService = new FakeAmazonNotificationService();
            MessageServices messageService = new MessageServices(_ctx, amazonNotificationService);
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);
            SecurityService securityService = new SecurityService();

            var transactionBatchGuid = Guid.NewGuid();
            var transactionAmount = 2.75;

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = false,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });
            _ctx.SaveChanges();

            var application = applicationService.AddApplication("Test", "http://www.test.com", true);
            var sender = userService.AddUser(application.ApiKey, "sender@paidthx.com", "pdthx123",
                "sender@paidthx.com", "1234");
            sender.SecurityPin = securityService.Encrypt("2589");
            userService.UpdateUser(sender);

            var senderPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Sender PaidThx",
                "053000219", "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderPaymentAccount);

            var recipient = userService.AddUser(application.ApiKey, "recipient@paidthx.com", "pdthx123",
                "recipient@paidthx.com", "1234");

            var recipientPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Recipient PaidThx",
    "053000219", "1234123412", "Savings");

            recipient.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            recipient.PaymentAccounts.Add(recipientPaymentAccount);

            var message = messageService.AddMessage(application.ApiKey.ToString(), sender.EmailAddress, recipient.EmailAddress,
                senderPaymentAccount.Id.ToString(), transactionAmount, "Test Payment", "Payment", "2589");

            message.Recipient = userService.FindUserByEmailAddress(recipient.EmailAddress);

            //transactionBatchService.BatchTransactions(message);

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreEqual(transactionBatchGuid, transactionBatch.Id);
            Assert.AreEqual(2, transactionBatch.Transactions.Count);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfDeposits);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfWithdrawals);

        }
        [TestMethod]
        public void WhenBatchingMessageWithNoOpenBatchesWithWithdrawlAndDepositTransactionsThenTransactionsInBatchIs2()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            IAmazonNotificationService amazonNotificationService = new FakeAmazonNotificationService();
            MessageServices messageService = new MessageServices(_ctx, amazonNotificationService);
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);
            SecurityService securityService = new SecurityService();

            var transactionBatchGuid = Guid.NewGuid();
            var transactionAmount = 2.75;

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = true,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });
            _ctx.SaveChanges();

            var application = applicationService.AddApplication("Test", "http://www.test.com", true);
            var sender = userService.AddUser(application.ApiKey, "sender@paidthx.com", "pdthx123",
                "sender@paidthx.com", "1234");
            sender.SecurityPin = securityService.Encrypt("2589");
            userService.UpdateUser(sender);

            var senderPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Sender PaidThx",
                "053000219", "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderPaymentAccount);

            var recipient = userService.AddUser(application.ApiKey, "recipient@paidthx.com", "pdthx123",
                "recipient@paidthx.com", "1234");

            var recipientPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Recipient PaidThx",
    "053000219", "1234123412", "Savings");

            recipient.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            recipient.PaymentAccounts.Add(recipientPaymentAccount);

            var message = messageService.AddMessage(application.ApiKey.ToString(), sender.EmailAddress, recipient.EmailAddress,
                senderPaymentAccount.Id.ToString(), transactionAmount, "Test Payment", "Payment", "2589");

            message.Recipient = userService.FindUserByEmailAddress(recipient.EmailAddress);

            //transactionBatchService.BatchTransactions(message);

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreNotEqual(transactionBatchGuid, transactionBatch.Id);
            Assert.AreEqual(2, transactionBatch.Transactions.Count);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfDeposits);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfWithdrawals);
        }
        [TestMethod]
        public void WhenBatchingMessageWithOpenBatchWithOnlyWithdrawThenTransactionsInBatchIs1()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            IAmazonNotificationService amazonNotificationService = new FakeAmazonNotificationService();
            MessageServices messageService = new MessageServices(_ctx, amazonNotificationService);
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);
            SecurityService securityService = new SecurityService();

            var transactionBatchGuid = Guid.NewGuid();
            var transactionAmount = 2.75;

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = false,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });
            _ctx.SaveChanges();

            var application = applicationService.AddApplication("Test", "http://www.test.com", true);
            var sender = userService.AddUser(application.ApiKey, "sender@paidthx.com", "pdthx123",
                "sender@paidthx.com", "1234");
            sender.SecurityPin = securityService.Encrypt("2589");
            userService.UpdateUser(sender);

            var senderPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Sender PaidThx",
                "053000219", "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderPaymentAccount);

            var message = messageService.AddMessage(application.ApiKey.ToString(), sender.EmailAddress, "recipient@paidthx.com",
                senderPaymentAccount.Id.ToString(), transactionAmount, "Test Payment", "Payment", "2589");

            //transactionBatchService.BatchTransactions(message);

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreEqual(transactionBatchGuid, transactionBatch.Id);
            Assert.AreEqual(2, transactionBatch.Transactions.Count);
            Assert.AreEqual(0, transactionBatch.TotalNumberOfDeposits);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfWithdrawals);
        }
        [TestMethod]
        public void WhenBatchingMessageWithNoOpenBatchWithWithdrawlOnlyThenTransactionsInBatchIs1()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            IAmazonNotificationService amazonNotificationService = new FakeAmazonNotificationService();
            MessageServices messageService = new MessageServices(_ctx, amazonNotificationService); 
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);
            SecurityService securityService = new SecurityService();

            var transactionBatchGuid = Guid.NewGuid();
            var transactionAmount = 2.75;

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = true,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });
            _ctx.SaveChanges();

            var application = applicationService.AddApplication("Test", "http://www.test.com", true);
            var sender = userService.AddUser(application.ApiKey, "sender@paidthx.com", "pdthx123",
                "sender@paidthx.com", "1234");
            sender.SecurityPin = securityService.Encrypt("2589");
            userService.UpdateUser(sender);

            var senderPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Sender PaidThx",
                "053000219", "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderPaymentAccount);

            var message = messageService.AddMessage(application.ApiKey.ToString(), sender.EmailAddress, "recipient@paidthx.com",
                senderPaymentAccount.Id.ToString(), transactionAmount, "Test Payment", "Payment", "2589");

            //transactionBatchService.BatchTransactions(message);

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreNotEqual(transactionBatchGuid, transactionBatch.Id);
            Assert.AreEqual(2, transactionBatch.Transactions.Count);
            Assert.AreEqual(0, transactionBatch.TotalNumberOfDeposits);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfWithdrawals);
        }
        [TestMethod]
        public void WhenBatching3TransactionsWithOpenBatchTransactionsInBatchIs3()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            IAmazonNotificationService amazonNotificationService = new FakeAmazonNotificationService();
            MessageServices messageService = new MessageServices(_ctx, amazonNotificationService); 
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);

            var transactionBatchGuid = Guid.NewGuid();
            var transactionAmount = 2.75;

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = true,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });
            _ctx.SaveChanges();

            var application = applicationService.AddApplication("Test", "http://www.test.com", true);
            var sender = userService.AddUser(application.ApiKey, "sender@paidthx.com", "pdthx123",
                "sender@paidthx.com", "1234");
            sender.SecurityPin = "2589";
            userService.UpdateUser(sender);

            var senderPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Sender PaidThx",
                "053000219", "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderPaymentAccount);

            var transaction1 = new Domain.Transaction()
            {
                Amount = 1,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Withdrawal,
                User = sender
            };
            var transaction2 = new Domain.Transaction()
            {
                Amount = 2,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Withdrawal,
                User = sender
            };
            var transaction3 = new Domain.Transaction()
            {
                Amount = 3,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Deposit,
                User = sender
            };

            //transactionBatchService.BatchTransactions(new List<Transaction>()
            //{
            //    transaction1,
            //    transaction2,
            //    transaction3
            //});

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreEqual(3, transactionBatch.Transactions.Count);
            Assert.AreEqual(1, transactionBatch.TotalNumberOfDeposits);
            Assert.AreEqual(2, transactionBatch.TotalNumberOfWithdrawals);
        }
        [TestMethod]
        public void WhenBatching5TransactionsWithOpenBatchTransactionsInBatchIs5()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            IAmazonNotificationService amazonNotificationService = new FakeAmazonNotificationService();
            MessageServices messageService = new MessageServices(_ctx, amazonNotificationService); 
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);

            var transactionBatchGuid = Guid.NewGuid();
            var transactionAmount = 2.75;

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = true,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });
            _ctx.SaveChanges();

            var application = applicationService.AddApplication("Test", "http://www.test.com", true);
            var sender = userService.AddUser(application.ApiKey, "sender@paidthx.com", "pdthx123",
                "sender@paidthx.com", "1234");
            sender.SecurityPin = "2589";
            userService.UpdateUser(sender);

            var senderPaymentAccount = paymentAccountService.AddPaymentAccount(sender.UserId.ToString(), "Sender PaidThx",
                "053000219", "1234123412", "Checking");

            sender.PaymentAccounts = new System.Collections.ObjectModel.Collection<PaymentAccount>();
            sender.PaymentAccounts.Add(senderPaymentAccount);

            var transaction1 = new Domain.Transaction()
            {
                Amount = 1,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Withdrawal,
                User = sender
            };
            var transaction2 = new Domain.Transaction()
            {
                Amount = 2,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Withdrawal,
                User = sender
            };
            var transaction3 = new Domain.Transaction()
            {
                Amount = 3,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Deposit,
                User = sender
            };
            var transaction4 = new Domain.Transaction()
            {
                Amount = 4,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Deposit,
                User = sender
            };
            var transaction5 = new Domain.Transaction()
            {
                Amount = 5,
                Category = TransactionCategory.Payment,
                FromAccount = senderPaymentAccount,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Deposit,
                User = sender
            };
            //transactionBatchService.BatchTransactions(new List<Transaction>()
            //{
            //    transaction1,
            //    transaction2,
            //    transaction3,
            //    transaction4,
            //    transaction5
            //});

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreEqual(5, transactionBatch.Transactions.Count);
            Assert.AreEqual(3, transactionBatch.TotalNumberOfDeposits);
            Assert.AreEqual(2, transactionBatch.TotalNumberOfWithdrawals);
        }
        [TestMethod]
        public void WhenGettingOpenBatchWithAnOpenBatchCurrentBatchIsReturned()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);

            var transactionBatchGuid = Guid.NewGuid();

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = false,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreEqual(transactionBatchGuid, transactionBatch.Id);
        }
        [TestMethod]
        public void WhenGettingOpenBatchWithNoOpenBatchesNewBatchIsCreated()
        {
            ApplicationService applicationService = new ApplicationService(_ctx);
            UserService userService = new UserService(_ctx);
            PaymentAccountService paymentAccountService = new PaymentAccountService(_ctx);
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);

            var transactionBatchGuid = Guid.NewGuid();

            _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = transactionBatchGuid,
                IsClosed = true,
                TotalDepositAmount = 0,
                TotalNumberOfDeposits = 0,
                TotalWithdrawalAmount = 0,
                TotalNumberOfWithdrawals = 0,
                Transactions = new Collection<Transaction>()
            });

            var transactionBatch = transactionBatchService.GetOpenBatch();

            Assert.AreNotEqual(transactionBatchGuid, transactionBatch.Id);
        }
        [TestMethod]
        public void WhenRemovingSingleTransactionFromBatchBatchReturnsTrue()
        {
            TransactionBatchService transactionBatchService = new TransactionBatchService(_ctx, _logger);

            var transactionBatch = _ctx.TransactionBatches.Add(new TransactionBatch()
            {
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                IsClosed = false,
                Transactions = new Collection<Transaction>()
            });

            //transactionBatchService.BatchTransactions(new List<Transaction>()
            //{
            //    new Transaction() {
            //        Amount = 5,
            //        Category = TransactionCategory.Payment,
            //        Id = Guid.NewGuid(),
            //        PaymentChannelType = PaymentChannelType.Single,
            //        Status = TransactionStatus.Pending,
            //        StandardEntryClass = StandardEntryClass.Web,
            //        Type = TransactionType.Deposit
            //    },
            //    new Transaction() {
            //        Amount = 10,
            //        Category = TransactionCategory.Payment,
            //        Id = Guid.NewGuid(),
            //        PaymentChannelType = PaymentChannelType.Single,
            //        Status = TransactionStatus.Pending,
            //        StandardEntryClass = StandardEntryClass.Web,
            //        Type = TransactionType.Deposit
            //    },
            //    new Transaction() {
            //        Amount = 15,
            //        Category = TransactionCategory.Payment,
            //        Id = Guid.NewGuid(),
            //        PaymentChannelType = PaymentChannelType.Single,
            //        Status = TransactionStatus.Pending,
            //        StandardEntryClass = StandardEntryClass.Web,
            //        Type = TransactionType.Deposit
            //    },
            //    new Transaction() {
            //        Amount = 20,
            //        Category = TransactionCategory.Payment,
            //        Id = Guid.NewGuid(),
            //        PaymentChannelType = PaymentChannelType.Single,
            //        Status = TransactionStatus.Pending,
            //        StandardEntryClass = StandardEntryClass.Web,
            //        Type = TransactionType.Deposit
            //    }
            //});

            var transactionToRemove = new Transaction()
            {
                Amount = 5,
                Category = TransactionCategory.Payment,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                Status = TransactionStatus.Pending,
                StandardEntryClass = StandardEntryClass.Web,
                Type = TransactionType.Deposit
            };

            //transactionBatchService.BatchTransactions(new List<Transaction>() {
            //    transactionToRemove
            //});

            var numberOfTransactionInBatch = transactionBatch.Transactions.Count;

            bool result = false; // transactionBatchService.RemoveTransactionsFromBatch(transactionToRemove);

            Assert.IsTrue(result);
        }
    }
}
