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
                try
                {
                    var messageService = new MessageServices();

                    var message = messageService.GetMessage(messageId);
                    ctx.Messages.Attach(message);

                    message.LastUpdatedDate = System.DateTime.Now;
                    message.Status = Domain.PaystreamMessageStatus.AcceptedRequest;

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Payment Message Task. Exception: {0}. Stack Trace: {1}", ex.Message, ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Payment Message Task. Inner Exception: {0}. Stack Trace: {1}", innerException.Message, innerException.StackTrace));
                        innerException = innerException.InnerException;
                    }
                }
            }
        }
    }
}
