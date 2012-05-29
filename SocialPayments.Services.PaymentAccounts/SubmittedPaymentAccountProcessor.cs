using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DomainServices;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.Services.PaymentAccounts
{
    public class SubmittedPaymentAccountProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        private TransactionBatchService _transactionBatchService;
        private PaymentAccountVerificationService _paymentAccountVerificationService;
        private IEmailService _emailService;

        private string _emailSubject = "You've added a new bank account";

        public SubmittedPaymentAccountProcessor()
        {
            _ctx  = new DataLayer.Context();
            _logger = LogManager.GetCurrentClassLogger();

            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _paymentAccountVerificationService = new PaymentAccountVerificationService(_ctx, _logger);
            _emailService = new EmailService(_ctx, _logger);
        }
        public SubmittedPaymentAccountProcessor(IDbContext context, IEmailService emailService)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();

            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _paymentAccountVerificationService = new PaymentAccountVerificationService(_ctx, _logger);
            _emailService = new EmailService(_ctx, _logger);
        }
        public bool Process(PaymentAccount paymentAccount)
        {
            //get random numbers between 10 and 49 that are not equal
            Random rand1 = new Random(10);
            Random rand2 = new Random(49);
            var depositAmount1 = (double)rand1.Next(10, 49)/100;
            var depositAmount2 = (double)rand2.Next(10, 49)/100;

            while (depositAmount1.Equals(depositAmount2))
            {
                depositAmount2 = (double)rand2.Next(10, 49) / 100;
            }

            var withdrawalAmount = depositAmount1 + depositAmount2;

            var estimatedSettlementDate = System.DateTime.Now.AddDays(5);
            //create transactions
            var deposit1 = _ctx.Transactions.Add(new Transaction()
            {
                Amount = depositAmount1,
                Category = TransactionCategory.AccountVerification,
                CreateDate = System.DateTime.Now,
                FromAccount = paymentAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Submitted,
                Type = TransactionType.Deposit,
                UserId = paymentAccount.UserId
            });
            var deposit2 = _ctx.Transactions.Add(new Transaction()
            {
                Amount = depositAmount2,
                Category = TransactionCategory.AccountVerification,
                CreateDate = System.DateTime.Now,
                FromAccount = paymentAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Submitted,
                Type = TransactionType.Deposit,
                UserId = paymentAccount.UserId
            });
            var withdrawal = _ctx.Transactions.Add(new Transaction()
            {
                Amount = withdrawalAmount,
                Category = TransactionCategory.AccountVerification,
                CreateDate = System.DateTime.Now,
                FromAccount = paymentAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                Status = TransactionStatus.Submitted,
                Type = TransactionType.Withdrawal,
                UserId = paymentAccount.UserId
            });

            //batch two deposits into account and one withdrawal for total deposit amount
            try
            {
                _transactionBatchService.BatchTransactions(new List<Transaction>() {
                    deposit1,
                    deposit2,
                    withdrawal
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //log to PaymentAccountVerification log table and estimate when settlement complete
            var paymentAccountVerification =  _paymentAccountVerificationService.AddVerification(paymentAccount.UserId.ToString(), paymentAccount.Id.ToString()
                , depositAmount1, depositAmount2, withdrawalAmount, System.DateTime.Now, estimatedSettlementDate);


            //first_name
            //last_name
            //acct_nickname
            //acct_lastfour
            //est_settle_date
            //est_settle_days
            //app_user  t/f indicating whether you have signed in from an app
            //send email to account owner
            _emailService.SendEmail(paymentAccount.User.EmailAddress, "Your New PaidThx Payment Account", "New ACH Account Setup", new List<KeyValuePair<string, string>>()
                
                {
                
                });

            return true;
        }
    }
}
