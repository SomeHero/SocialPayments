using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using NLog;
using System.Text.RegularExpressions;
using SocialPayments.Services.IMessageProcessor;
using SocialPayments.Domain;
namespace SocialPayments.Workflows.Messages
{
    public class MessageWorkflow
    {
        private Context _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private DomainServices.ApplicationService _applicationService;
        private DomainServices.TransactionBatchService _transactionBatchService;
        private DomainServices.EmailService _emailService;
        private DomainServices.SMSLogService _smsLogService;
        private DomainServices.SMSService _smsService;
                        
        public MessageWorkflow()
        {
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = new DomainServices.EmailService(_ctx);
            _applicationService= new ApplicationService();
            _smsLogService = new SMSLogService(_ctx);
            _smsService = new DomainServices.SMSService(_applicationService, _smsLogService, _ctx, _logger);
        }
        public MessageWorkflow(DomainServices.ApplicationService applicationServices, DomainServices.EmailService emailService,
            DomainServices.TransactionBatchService transactionBatchService, DomainServices.SMSLogService smsLogService,
            DomainServices.SMSService smsService)
        {
            _transactionBatchService = transactionBatchService;
            _emailService = emailService;
            _applicationService = applicationServices;
            _smsLogService = smsLogService;
            _smsService = smsService;
        }

        public void Execute(string id)
        {
            var message = GetMessage(id);

            if (message == null)
                throw new ArgumentException("Invalid Message Id", "Id");

            switch (message.MessageType)
            {
                case Domain.MessageType.Payment:
                    try
                    {
                        ProcessPayment(message);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    break;
                case Domain.MessageType.PaymentRequest:
                    try
                    {
                        ProcessPaymentRequest(message);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    break;
            }
        }

        private void ProcessPayment(Message message)
        {
            String smsMessage = "";
            switch (message.MessageStatus)
            {
                case Domain.MessageStatus.Submitted:

                    IMessageProcessor messageProcessor = new SocialPayments.Services.MessageProcessors.SubmittedPaymentMessageProcessor(_ctx);
                    messageProcessor.Process(message);

                    break;

                //remove associated transactions from batch and cancel transactions
                case Domain.MessageStatus.CancelPending:

                    break;
                case Domain.MessageStatus.Cancelled:
                    //terminal state
                    break;

                case Domain.MessageStatus.Completed:
                    //terminal state
                    break;

                case Domain.MessageStatus.Failed:
                    //terminal state

                    break;
                case Domain.MessageStatus.Pending:
                    //moves to Completed State by NACHA processor when NACHA file is created

                    break;

                case Domain.MessageStatus.Refunded:
                    //terminal state

                    break;
            }
        }

        private void ProcessPaymentRequest(Message message)
        {
            switch (message.MessageStatus)
            {
                case Domain.MessageStatus.Cancelled:
                    break;
                case Domain.MessageStatus.Completed:
                    break;
                case Domain.MessageStatus.Failed:
                    break;
                case Domain.MessageStatus.Pending:
                    break;
                case Domain.MessageStatus.Refunded:
                    break;
                case Domain.MessageStatus.Submitted:

                    //look for recipient
                    //send sms and email to recipient
                    //send sms and confirmation back to send
                    break;
                case Domain.MessageStatus.RequestAccepted:

                    //validate sender and recipient
                    //create withdraw and deposit records and batch
                    _transactionBatchService.BatchTransactions(message);

                    message.MessageStatus = Domain.MessageStatus.Pending;
                    message.LastUpdatedDate = System.DateTime.Now;
                       
                    _ctx.SaveChanges();

                    //send sms and email confirmations
                    break;

                case Domain.MessageStatus.RequestRejected:

                    //send sms and email to sender
                    break;
            }
        }

        private Message GetMessage(string id)
        {
            Guid messageId;

            Guid.TryParse(id, out messageId);

            if (messageId == null)
                return null;

            var message = _ctx.Messages
                .FirstOrDefault(m => m.Id.Equals(messageId));

            return message;
        }
    }
}
