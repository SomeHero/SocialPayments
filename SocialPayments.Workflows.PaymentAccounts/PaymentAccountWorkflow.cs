using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DomainServices;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.Services.PaymentAccounts;

namespace SocialPayments.Workflows.PaymentAccounts
{
    public class PaymentAccountWorkflow
    {
        private IDbContext _ctx;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private DomainServices.PaymentAccountService _paymentAccountService;
        private DomainServices.ApplicationService _applicationService;
        private DomainServices.TransactionBatchService _transactionBatchService;
        private DomainServices.Interfaces.IEmailService _emailService;
        private DomainServices.SMSLogService _smsLogService;
        private DomainServices.SMSService _smsService;



        public PaymentAccountWorkflow()
        {
            _ctx = new Context();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = new DomainServices.EmailService(_ctx);
            _applicationService= new ApplicationService(_ctx);
            _smsLogService = new SMSLogService(_ctx);
            _smsService = new DomainServices.SMSService(_applicationService, _smsLogService, _ctx, _logger);
        }
        public PaymentAccountWorkflow(IDbContext context)
        {
            _ctx = context;
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _emailService = new DomainServices.EmailService(_ctx);
            _applicationService = new ApplicationService(_ctx);
            _smsLogService = new SMSLogService(_ctx);
            _smsService = new DomainServices.SMSService(_applicationService, _smsLogService, _ctx, _logger);
        }
        public PaymentAccountWorkflow(DomainServices.ApplicationService applicationServices, DomainServices.Interfaces.IEmailService emailService,
            DomainServices.TransactionBatchService transactionBatchService, DomainServices.SMSLogService smsLogService,
            DomainServices.SMSService smsService)
        {
            _ctx = new Context();
            _transactionBatchService = transactionBatchService;
            _emailService = emailService;
            _applicationService = applicationServices;
            _smsLogService = smsLogService;
            _smsService = smsService;
        }

        public void Execute(string id)
        {
            Domain.PaymentAccount paymentAccount = _paymentAccountService.GetPaymentAccount(id);

            _logger.Log(LogLevel.Info, String.Format("Processing Payment Account {0} with Status {1}", paymentAccount.Id, paymentAccount.AccountStatus.ToString()));
            switch (paymentAccount.AccountStatus)
            {
                case Domain.AccountStatusType.Submitted:

                    SubmittedPaymentAccountProcessor processor = new SubmittedPaymentAccountProcessor(_ctx, _emailService);

                    processor.Process(paymentAccount);

                    break;
            }
            _logger.Log(LogLevel.Info, String.Format("Finished Processing Payment Account {0} with Status {1}", paymentAccount.Id, paymentAccount.AccountStatus.ToString()));
            
        }
    }
}
