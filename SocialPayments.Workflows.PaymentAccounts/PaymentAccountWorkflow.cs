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

        public PaymentAccountWorkflow()
        {
            _ctx = new Context();
            _paymentAccountService = new PaymentAccountService(_ctx);
        }
        public PaymentAccountWorkflow(IDbContext context)
        {
            _ctx = context;
            _paymentAccountService = new PaymentAccountService(_ctx);
        }
        public PaymentAccountWorkflow(DomainServices.ApplicationService applicationServices, DomainServices.Interfaces.IEmailService emailService,
            DomainServices.TransactionBatchService transactionBatchService, DomainServices.SMSLogService smsLogService,
            DomainServices.SMSService smsService)
        {
            _ctx = new Context();
            _paymentAccountService = new PaymentAccountService(_ctx);
        }

        public void Execute(string id)
        {
            Domain.PaymentAccount paymentAccount = _paymentAccountService.GetPaymentAccount(id);

            _logger.Log(LogLevel.Info, String.Format("Processing Payment Account {0} with Status {1}", paymentAccount.Id, paymentAccount.AccountStatus.ToString()));
            switch (paymentAccount.AccountStatus)
            {
                case Domain.AccountStatusType.Submitted:

                    SubmittedPaymentAccountProcessor processor = new SubmittedPaymentAccountProcessor();

                    processor.Process(paymentAccount);

                    break;
            }
            _logger.Log(LogLevel.Info, String.Format("Finished Processing Payment Account {0} with Status {1}", paymentAccount.Id, paymentAccount.AccountStatus.ToString()));
            
        }
    }
}
