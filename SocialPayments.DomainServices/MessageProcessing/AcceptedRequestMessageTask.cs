using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using NLog;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class AcceptedRequestMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Execute(Guid messageId)
        {
            using (var ctx = new Context())
            {
                var messageService = new MessageServices(ctx);

                var message = messageService.GetMessage(messageId);

                message.LastUpdatedDate = System.DateTime.Now;
                message.Status = Domain.PaystreamMessageStatus.AcceptedRequest;

                ctx.SaveChanges();
            }
        }
    }
}
