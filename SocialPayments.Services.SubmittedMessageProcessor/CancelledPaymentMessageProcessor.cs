using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.Domain;

namespace SocialPayments.Services.SubmittedMessageProcessor
{
    public class CancelledPaymentMessageProcessor : IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        public CancelledPaymentMessageProcessor() {
            _ctx  = new DataLayer.Context();
            _logger = LogManager.GetCurrentClassLogger();
        }

        public CancelledPaymentMessageProcessor(IDbContext context)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public bool Process(Message message)
        {

            if (message.MessageStatus != MessageStatus.Submitted || message.MessageStatus != MessageStatus.Pending)
                throw new Exception("Only payment with a status of submitted or pending can be cancelled.");

            foreach (var transaction in message.Transactions)
            {
                transaction.Status = TransactionStatus.Cancelled;
                transaction.LastUpdatedDate = System.DateTime.Now;
            }

            _ctx.SaveChanges();

            return true;
        }
    }
}
