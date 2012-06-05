using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.DomainServices;
using SocialPayments.DomainServices.UnitTests.Fakes;

namespace SocialPayments.Services.MessageProcessors.UnitTest
{
    [TestClass]
    public class AcceptPaymentRequestMessageProcessorTest
    {
        private IDbContext _ctx;
        private Logger _logger;
        private AcceptPaymentRequestMessageProcessor _acceptPaymentRequestMessageProccess;
        private SubmittedRequestMessageProcessor _submittedMessageProcessor;
        private ISMSService _smsService;
        private IEmailService _emailService;
        private TransactionBatchService _transactionBatchServices;

        public AcceptPaymentRequestMessageProcessorTest()
        {
            _ctx = new FakeDbContext();
            _logger = LogManager.GetCurrentClassLogger();
            _smsService = new FakeSMSService();
            _emailService = new FakeEmailService();
            _transactionBatchServices = new TransactionBatchService(_ctx, _logger);
            _submittedMessageProcessor = new SubmittedRequestMessageProcessor(_ctx, _emailService, _smsService);
            _acceptPaymentRequestMessageProccess = new AcceptPaymentRequestMessageProcessor(_ctx, _logger, _transactionBatchServices);
        }
        [TestMethod]
        public void WhenRequestIsAcceptedThenTwoTransactionsAreCreatedAndMessageStatusIsPending()
        {
            var senderEmail = "sender@paidthx.com";
            var senderMobile = "8043550001";
            var recipientEmail = "recipient@paidthx.com";
            var recipientMobile = "8043550002";

            //Setup a sender, sender account, recipient, recipient account
            var application = Mother.CreateApplication(_ctx);
            var sender = Mother.CreateSender(_ctx, application, senderEmail, senderMobile);
            var recipient = Mother.CreateRecipient(_ctx, application, recipientEmail, recipientMobile);
            var senderAccount = Mother.CreatePaymentAccount(_ctx, sender);
            var recipientAccount = Mother.CreatePaymentAccount(_ctx, recipient);

            //setup a Payment Request
            var message = Mother.CreateMessageWithKnownRecipient(_ctx, application, sender, recipient, sender.EmailAddress,
                recipientEmail, Domain.MessageType.PaymentRequest);
            //Process the payment request
            _submittedMessageProcessor.Process(message);

            //Change the payment request to AcceptPending
            message.MessageStatus = Domain.MessageStatus.RequestAcceptedPending;

            //Process the payment request
            _acceptPaymentRequestMessageProccess.Process(message);

            var transactionBatch = _transactionBatchServices.GetOpenBatch();

            //Assert there are two transaction in batch
            Assert.AreEqual(2, transactionBatch.Transactions.Count);

            //Assert that the Status of the Request is Pending
            Assert.AreEqual(Domain.MessageStatus.Pending, message.MessageStatus);
        }
    }
}
