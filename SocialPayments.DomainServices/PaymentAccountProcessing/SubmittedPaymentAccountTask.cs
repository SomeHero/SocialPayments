using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using NLog;
using System.Configuration;

namespace SocialPayments.DomainServices.PaymentAccountProcessing
{
    public class SubmittedPaymentAccountTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        private string _emailSubject = "You've added a new bank account";
        private string _templateName = "ACH Account Verification";
        private string _paymentAccountVerificationBaseUrl = ConfigurationManager.AppSettings["PaymentAccountVerificationBaseUrl"];

        public void Excecute(Guid paymentAccountId)
        {
            using (var ctx = new Context())
            {
                var messageService = new DomainServices.MessageServices();
                var paymentAccountService = new DomainServices.PaymentAccountService(ctx);
                var communicationService = new DomainServices.CommunicationService(ctx);
                var emailService = new DomainServices.EmailService(ctx);
                var userService = new DomainServices.UserService(ctx);
                var transactionBatchServices = new DomainServices.TransactionBatchService(ctx, _logger);

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

                var transactionBatch = transactionBatchServices.GetOpenBatch();

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
                    Amount = depositAmount2,
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

                try
                {
                    var communicationTemplate = communicationService.GetCommunicationTemplate("New_Bank_Account_Email");
                    var link = String.Format(_paymentAccountVerificationBaseUrl, paymentAccountVerification.Id);

                    //FIRST_NAME, LAST_NAME, BANK_NAME, BANK_ACCOUNT_NAME, BANK_ACCOUNT_STATUS, LINK_VERIFY, LINK_VERIFY_INSTANT
                    emailService.SendEmail(paymentAccount.User.EmailAddress, "Your bank account has been added", communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                    {
                            new KeyValuePair<string, string>("FIRST_NAME", userService.GetSenderName(paymentAccount.User)),
                            new KeyValuePair<string, string>("LAST_NAME", ""),
                            new KeyValuePair<string, string>("BANK_NAME", paymentAccount.BankName),
                            new KeyValuePair<string, string>("BANK_ICON_URL", paymentAccount.BankIconURL),
                            new KeyValuePair<string, string>("BANK_ACCOUNT_NAME",  paymentAccount.Nickname),
                            new KeyValuePair<string, string>("BANK_ACCOUNT_STATUS",  paymentAccount.AccountStatus.ToString()),
                            new KeyValuePair<string, string>("LINK_VERIFY",  link),           
                            new KeyValuePair<string, string>("LINK_VERIFY_INSTANT",  ""),      
                    });

                    paymentAccountVerification.Status = PaymentAccountVerificationStatus.Delivered;
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception Sending Bank Account Verification Link to {0}. {1}", paymentAccount.User.EmailAddress, ex.Message));
                }

                var messages = messageService.GetMessages(paymentAccount.User.UserId);

                _logger.Log(LogLevel.Info, String.Format("Processing {0} notified payment(s) from user {1}.",messages.Count, paymentAccount.User.UserName));

                foreach (var message in messages)
                {
                    if (message.Status == PaystreamMessageStatus.NotifiedPayment)
                    {
                        _logger.Log(LogLevel.Info, String.Format("Batch Deposit Transaction for Message {0} into PaymentAccount {1}.", message.Id, paymentAccount.Id));

                        try
                        {
                            message.Status = PaystreamMessageStatus.ProcessingPayment;
                            //Add the withdrawal transaction
                            message.Payment.Transactions.Add(new Domain.Transaction()
                            {
                                AccountNumber = paymentAccount.AccountNumber,
                                Amount = message.Payment.Amount,
                                AccountType = Domain.AccountType.Checking,
                                ACHTransactionId = "",
                                CreateDate = System.DateTime.Now,
                                Id = Guid.NewGuid(),
                                IndividualIdentifier = userService.GetSenderName(message.Sender),
                                NameOnAccount = paymentAccount.NameOnAccount,
                                PaymentChannelType = PaymentChannelType.Single,
                                RoutingNumber = paymentAccount.RoutingNumber,
                                StandardEntryClass = StandardEntryClass.Web,
                                Status = TransactionStatus.Pending,
                                Type = TransactionType.Withdrawal,
                                TransactionBatch = transactionBatch,
                                Payment = message.Payment
                            });

                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, String.Format("Exception Batching Deposit Transaction for Message {0} into PaymentAccount {1}. Exception: {2}", message.Id, paymentAccount.Id, ex.Message));
                        }
                    }
                }

                ctx.SaveChanges();

            }
        }
    }
}
