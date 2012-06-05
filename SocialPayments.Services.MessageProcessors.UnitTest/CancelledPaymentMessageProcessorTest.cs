using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.UnitTests.Fakes;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.DomainServices;
using NLog;

namespace SocialPayments.Services.MessageProcessors.UnitTest
{
    [TestClass]
    public class CancelledPaymentMessageProcessorTest
    {
        private IDbContext _ctx;
        private Logger _logger;
        private CancelledPaymentMessageProcessor _cancelledMessageProcessor;
        private SubmittedPaymentMessageProcessor _submittedMessageProcessor;
        private ISMSService _smsService;
        private IEmailService _emailService;
        private TransactionBatchService _transactionBatchServices;

        [TestMethod]
        public void WhenAMessageInPendingStatusIsCancelledPendingThenMessageStatusIsUpdatedToCancelledAndTransactionIsRemovedFromBatch()
        {
            Guid messageId = Guid.NewGuid();
            Guid apiKey = Guid.NewGuid();
            Guid senderId = Guid.NewGuid();
            Guid senderAccountId = Guid.NewGuid();

            _ctx = new FakeDbContext();
            _logger = LogManager.GetCurrentClassLogger();
            _emailService = new FakeEmailService();
            _smsService = new FakeSMSService();
            _submittedMessageProcessor = new SubmittedPaymentMessageProcessor(_ctx, _emailService, _smsService);
            _cancelledMessageProcessor = new CancelledPaymentMessageProcessor(_ctx, _emailService, _smsService);
            _transactionBatchServices = new TransactionBatchService(_ctx, _logger);

            var senderEmail = "james@paidthx.com";
            var senderMobileNumber = "8043879693";
            var recipientEmail = "jrhodes2705@gmail.com";
            var recipientMobileNumber = "8043550001";

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
                EmailAddress = senderEmail,
                Limit = 100,
                MobileNumber = senderMobileNumber,
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
                RecipientUri = recipientEmail,
                Sender = sender,
                SenderId = senderId,
                SenderAccount = senderAccount,
                SenderAccountId = senderAccountId,
                SenderUri = senderEmail,
                Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>()
            });

            _ctx.SaveChanges();

            _submittedMessageProcessor.Process(message);

            message.MessageStatus = Domain.MessageStatus.CancelPending;

            var transactionBatch = _transactionBatchServices.GetOpenBatch();
            var transactionsInBatchBeforeCancel = transactionBatch.Transactions.Count;

            _cancelledMessageProcessor.Process(message);

            Assert.AreEqual(Domain.MessageStatus.Cancelled, message.MessageStatus);
            Assert.AreEqual(transactionBatch.Transactions.Count, transactionsInBatchBeforeCancel - 1);

        }
    }
}
