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
using SocialPayments.DataLayer.Interfaces;
namespace SocialPayments.Workflows.Messages
{
    public class MessageWorkflow
    {
        private Context _ctx;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private DomainServices.ApplicationService _applicationService;
        private DomainServices.TransactionBatchService _transactionBatchService;
        private DomainServices.EmailService _emailService;
        private DomainServices.SMSLogService _smsLogService;
        private DomainServices.SMSService _smsService;
                        
        public MessageWorkflow()
        {
            _ctx = new Context();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = new DomainServices.EmailService(_ctx);
            _applicationService= new ApplicationService(_ctx);
            _smsLogService = new SMSLogService(_ctx);
            _smsService = new DomainServices.SMSService(_applicationService, _smsLogService, _ctx, _logger);
        }
        public MessageWorkflow(IDbContext context)
        {
            _ctx = new Context();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = new DomainServices.EmailService(_ctx);
            _applicationService = new ApplicationService(_ctx);
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


            _logger.Log(LogLevel.Info, String.Format("Processing Message {0} of Type {1} with Status {2}", message.Id, message.MessageType.ToString(), message.Status.ToString()));
            switch (message.MessageType)
            {
                case Domain.MessageType.Payment:
                    try
                    {
                        ProcessPayment(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Error Processing Message {0} of Type {1} with Status {2}", message.Id, message.MessageType.ToString(), message.Status.ToString()));
            
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

            _logger.Log(LogLevel.Info, String.Format("Finished Processing Message {0} of Type {1} with Status {2}", message.Id, message.MessageType.ToString(), message.Status.ToString()));
            
        }

        private void ProcessPayment(Message message)
        {
            _logger.Log(LogLevel.Error, String.Format("Processing Payment with Status {0} in {1}", message.Status, message.WorkflowStatus));
            
            IMessageProcessor messageProcessor;

            switch (message.Status)
            {
                case Domain.PaystreamMessageStatus.Processing:

                    _logger.Log(LogLevel.Error, String.Format("Starting Submitted Payment Message Processor"));
            
                    messageProcessor = new SocialPayments.Services.MessageProcessors.SubmittedPaymentMessageProcessor(_ctx, _emailService, _smsService);
                    messageProcessor.Process(message);

                    break;

                //remove associated transactions from batch and cancel transactions

                case Domain.PaystreamMessageStatus.Cancelled:
                     _logger.Log(LogLevel.Error, String.Format("Starting Cancelled Payment Message Processor"));
            
                    messageProcessor = new SocialPayments.Services.MessageProcessors.CancelledPaymentMessageProcessor(_ctx, _emailService, _smsService);
                    messageProcessor.Process(message);

                    break;

                case Domain.PaystreamMessageStatus.Complete:
                    //terminal state
                    break;

            }
        }

        private void ProcessPaymentRequest(Message message)
        {
            switch (message.Status)
            {
                case Domain.PaystreamMessageStatus.Cancelled:
                    break;
                case Domain.PaystreamMessageStatus.Complete:
                    break;
                case Domain.PaystreamMessageStatus.Processing:
                    _logger.Log(LogLevel.Error, String.Format("Starting Request Message Processor"));
            
                    IMessageProcessor messageProcessor = new SocialPayments.Services.MessageProcessors.SubmittedRequestMessageProcessor(_ctx);
                    messageProcessor.Process(message);

                    break;
                case Domain.PaystreamMessageStatus.Accepted:

                    //validate sender and recipient
                    //create withdraw and deposit records and batch
                    //_transactionBatchService.BatchTransactions(message);

                    message.Status = PaystreamMessageStatus.Accepted;
                    message.LastUpdatedDate = System.DateTime.Now;
                       
                    _ctx.SaveChanges();

                    //send sms and email confirmations
                    break;

                case Domain.PaystreamMessageStatus.Rejected:

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
