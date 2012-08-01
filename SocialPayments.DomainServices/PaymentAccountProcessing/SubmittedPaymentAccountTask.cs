using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using NLog;

namespace SocialPayments.DomainServices.PaymentAccountProcessing
{
    public class SubmittedPaymentAccountTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        private string _emailSubject = "You've added a new bank account";
        private string _templateName = "ACH Account Verification";

        public void Excecute(Guid paymentAccountId)
        {
            using (var ctx = new Context())
            {
                var paymentAccountService = new DomainServices.PaymentAccountService(ctx);
                var emailService = new DomainServices.EmailService(ctx);

                var paymentAccount = paymentAccountService.GetPaymentAccount(paymentAccountId);

                //get random numbers between 10 and 49 that are not equal
                var random = new Random();

                var depositAmount1 = (double)random.Next(10, 49) / 100;
                var depositAmount2 = (double)random.Next(10, 49) / 100;

                while (depositAmount1.Equals(depositAmount2))
                {
                    depositAmount2 = (double)random.Next(10, 49) / 100;
                }

                var withdrawalAmount = depositAmount1 + depositAmount2;

                var sentDate = System.DateTime.Now;
                var estimatedSettlementDate = System.DateTime.Now.AddDays(5);

                var transactionBatchService = new TransactionBatchService(ctx, _logger);

                var transactionBatch = transactionBatchService.GetOpenBatch();

                //validate that user is owned by paymentAccount
                var paymentAccountVerification = ctx.PaymentAccountVerifications.Add(new PaymentAccountVerification()
                {
                    DepositAmount1 = depositAmount1,
                    DepositAmount2 = depositAmount2,
                    WithdrawalAmount = withdrawalAmount,
                    EstimatedSettlementDate = estimatedSettlementDate,
                    Id = Guid.NewGuid(),
                    PaymentAccount = paymentAccount,
                    Sent = sentDate,
                    Status = PaymentAccountVerificationStatus.Submitted
                });

                ctx.Transactions.Add(
                        new Domain.Transaction()
                        {

                            AccountNumber = paymentAccount.AccountNumber,
                            Amount = depositAmount1,
                            AccountType = Domain.AccountType.Checking,
                            ACHTransactionId = "",
                            CreateDate = System.DateTime.Now,
                            Id = Guid.NewGuid(),
                            IndividualIdentifier = "PaidThx",
                            NameOnAccount = paymentAccount.NameOnAccount,
                            PaymentChannelType = PaymentChannelType.Single,
                            RoutingNumber = paymentAccount.RoutingNumber,
                            StandardEntryClass = StandardEntryClass.Web,
                            Status = TransactionStatus.Pending,
                            Type = TransactionType.Deposit,
                            TransactionBatch = transactionBatch
                        });

                ctx.Transactions.Add(new Domain.Transaction()
                {

                    AccountNumber = paymentAccount.AccountNumber,
                    Amount = depositAmount1,
                    AccountType = Domain.AccountType.Checking,
                    ACHTransactionId = "",
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    IndividualIdentifier = "PaidThx",
                    NameOnAccount = paymentAccount.NameOnAccount,
                    PaymentChannelType = PaymentChannelType.Single,
                    RoutingNumber = paymentAccount.RoutingNumber,
                    StandardEntryClass = StandardEntryClass.Web,
                    Status = TransactionStatus.Pending,
                    Type = TransactionType.Deposit,
                    TransactionBatch = transactionBatch
                });

                ctx.Transactions.Add(new Domain.Transaction()
                {
                    AccountNumber = paymentAccount.AccountNumber,
                    Amount = withdrawalAmount,
                    AccountType = Domain.AccountType.Checking,
                    ACHTransactionId = "",
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    IndividualIdentifier = "PaidThx",
                    NameOnAccount = paymentAccount.NameOnAccount,
                    PaymentChannelType = PaymentChannelType.Single,
                    RoutingNumber = paymentAccount.RoutingNumber,
                    StandardEntryClass = StandardEntryClass.Web,
                    Status = TransactionStatus.Pending,
                    Type = TransactionType.Withdrawal,
                    TransactionBatch = transactionBatch
                });

                paymentAccount.AccountStatus = AccountStatusType.PendingActivation;

                ctx.SaveChanges();

                emailService.SendEmail(paymentAccount.User.EmailAddress, _emailSubject, _templateName,
                    new List<KeyValuePair<string, string>>());
            }
        }
    }
}
