using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.DomainServices;
using SocialPayments.DataLayer;

namespace SocialPayments.Services.MessageProcessors
{

    public class AcceptPaymentRequestMessageProcessor : IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;
        private TransactionBatchService _transactionBatchService;

        public AcceptPaymentRequestMessageProcessor()
        {
            _ctx = new Context();
            _logger = LogManager.GetCurrentClassLogger();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
        }
        public AcceptPaymentRequestMessageProcessor(IDbContext context, Logger logger, TransactionBatchService transactionBatchService)
        {
            _ctx = context;
            _logger = logger;
            _transactionBatchService = transactionBatchService;
        }
        public bool Process(Domain.Message message)
        {
           
            //batch payment
            var transactions =  _transactionBatchService.BatchTransactions(message);

            message.LastUpdatedDate = System.DateTime.Now;
            message.MessageStatus = Domain.MessageStatus.Pending;

            _ctx.SaveChanges();

            //send email/sms

            return true;
        }
    }
}
