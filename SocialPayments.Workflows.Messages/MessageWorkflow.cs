using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using NLog;
using System.Text.RegularExpressions;
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
        
        private String _recipientSMSMoneyReceived = "{1} just sent you {0:C} using PaidThx.  Go to {2} to complete this transaction.";
        private String _senderSMSMoneySent = "Your payment of {0:C} was sent to {1}.";
        private String _reciepientSMSRequestReceived = "{0} sent you a request for {0:C} using PaidThx.  Go to {2} to complete this transaction.";
        private String _senderSMSReqeustSent = "Your request for {0:C} was submitted to an unregistered user at {1}.";

        private String _recipientEmailMoneyReceivedSubject = "{1} just sent your {0:C} using PaidThx.";
        private String _senderEmailMoneySentSubject = "Confirmation of Your Payment to {0}.";
        private String _recipientEmailMoneySentSubject = "{1} just sent you a request for {0:C} using PaidThx";
        private String _senderEmailRequestSentSubject = "Confirmation of Your Request to {0}.";

        private String _recipientEmailMoneyReceived ="You received {0:C} from {1}.";
        private String _senderEmailMoneySent = "Your payment in the amount of {0:C} was delivered to {1}.";
        private String _recipientEmailRequestReceived = "You received a request for {0:C} from {1}..";
        private String _senderEmailRequestSent = "Your request for {0:C} was delivered to {1}.";

        private String _link = "http://beta.paidthx.com/mobile/{0}";
                        
        public MessageWorkflow()
        {
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = new DomainServices.EmailService(_ctx);
            _applicationService= new ApplicationService();
            _smsLogService = new SMSLogService();
            _smsService = new DomainServices.SMSService(_applicationService, _smsLogService, _ctx, _logger);
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

        private void ProcessPayment(Domain.Message message)
        {
            String smsMessage = "";
            switch (message.MessageStatus)
            {
                case Domain.MessageStatus.Submitted:
                    
                    try {
                        _transactionBatchService.BatchTransactions(message);
                        
                        _ctx.SaveChanges();


                        smsMessage = String.Format(_senderSMSMoneySent, message.Amount, message.RecipientUri);
                        _smsService.SendSMS(message.ApiKey, message.SenderUri, smsMessage);

                        String link = String.Format(_link, message.Id.ToString());

                        smsMessage = String.Format(_recipientSMSMoneyReceived, message.Amount, message.SenderUri, link);
                        _smsService.SendSMS(message.ApiKey, message.RecipientUri, smsMessage);

                    } catch(Exception ex) {
                        throw ex;
                    }

                    break;

                //remove associated transactions from batch and cancel transactions
                case Domain.MessageStatus.CancelPending:
                    try {
                        _transactionBatchService.RemoveFromBatch(message);

                        message.MessageStatus = Domain.MessageStatus.Cancelled;
                        message.LastUpdatedDate = System.DateTime.Now;

                        _ctx.SaveChanges();
                    }
                    catch(Exception ex) {

                    }
                    break;
                case Domain.MessageStatus.Cancelled:
                    //terminal state

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

        private void ProcessPaymentRequest(Domain.Message message)
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

        private Domain.Message GetMessage(string id)
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
