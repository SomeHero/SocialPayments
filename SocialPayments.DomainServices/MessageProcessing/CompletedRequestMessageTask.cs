using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class CompletedRequestMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Execute(Guid messageId)
        { }
    }
}
