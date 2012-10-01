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
    public class RejectPaymentRequestMessageProcessor: IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        public RejectPaymentRequestMessageProcessor()
        {
            _ctx = new Context();
            _logger = LogManager.GetCurrentClassLogger();
        }
        public RejectPaymentRequestMessageProcessor(IDbContext context, Logger logger)
        {
            _ctx = context;
            _logger = logger;
        }

        public bool Process(Domain.Message message)
        {
            //change status of message
            message.LastUpdatedDate = System.DateTime.Now;
            //message.Status = Domain.PaystreamMessageStatus.RequestRejected;

            _ctx.SaveChanges();

            //send email/sms

            return true;
        }
    }
}
