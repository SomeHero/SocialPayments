using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.Services.MessageProcessors
{
    public class CancelledPaymentMessageProcessor : IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;
        private TransactionBatchService _transactionBatchService;
        private IEmailService _emailService;
        private ISMSService _smsService;

        public CancelledPaymentMessageProcessor() {
            _ctx  = new DataLayer.Context();
            _logger = LogManager.GetCurrentClassLogger();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
        }

        public CancelledPaymentMessageProcessor(IDbContext context, IEmailService emailService, ISMSService smsService)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = emailService;
            _smsService = smsService;
        }

        public bool Process(Message message)
        {
            _logger.Log(LogLevel.Info, String.Format("Processing Cancel Message for Message {0}", message.Id.ToString()));

            bool removeSuccess = false;
            try
            {
                
                foreach (var transaction in message.Transactions)
                {
                    removeSuccess = _transactionBatchService.RemoveFromBatch(transaction);

                    if(!removeSuccess)
                        break;

                    transaction.Status = TransactionStatus.Cancelled;
                    transaction.LastUpdatedDate = System.DateTime.Now;
                }

                if (removeSuccess)
                {
                    message.MessageStatus = MessageStatus.Cancelled;

                    _ctx.SaveChanges();

                    return false;
                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Processing Cancelled Message {0}. {1}", message.Id.ToString(), ex.Message));

                return false;
            }

            //send confirmation

            return true;
        }
    }
}
