using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using NLog;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices.UserProcessing
{
    public class RegisteredUserTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Excecute(User user)
        {
            _logger.Log(LogLevel.Info, string.Format("Processing Registered User for {0}, Starting.", user.UserId));

            using (var ctx = new Context())
            {
                var transactionBatchService = new DomainServices.TransactionBatchService(ctx, _logger);

                try
                {

                    if (!String.IsNullOrEmpty(user.MobileNumber))
                    {

                        user.UserStatus = UserStatus.Active;

                        var messages = ctx.Messages
                            .Where(m => (m.RecipientUri == user.EmailAddress || m.RecipientUri == user.MobileNumber) && m.StatusValue.Equals((int)PaystreamMessageStatus.Processing))
                            .ToList();

                        var transactionBatch = transactionBatchService.GetOpenBatch();

                        _logger.Log(LogLevel.Info, string.Format("Processing Registered User for {0}, Found {1} Messages.", user.UserId, messages.Count));

                        foreach (var message in messages)
                        {
                            message.Payment.RecipientAccount = user.PreferredReceiveAccount;
                            
                            ctx.Transactions.Add(new Domain.Transaction()
                            {
                                Amount = message.Amount,
                                CreateDate = System.DateTime.Now,
                                AccountNumber = user.PreferredReceiveAccount.AccountNumber,
                                RoutingNumber = user.PreferredReceiveAccount.RoutingNumber,
                                NameOnAccount = user.PreferredReceiveAccount.NameOnAccount,
                                AccountType = Domain.AccountType.Checking,
                                Id = Guid.NewGuid(),
                                PaymentChannelType = PaymentChannelType.Single,
                                StandardEntryClass = StandardEntryClass.Web,
                                Status = TransactionStatus.Pending,
                                Type = TransactionType.Deposit,
                                TransactionBatch = transactionBatch
                            });

                        }

                        ctx.SaveChanges();

                        //send registration email
                        //string emailSubject = _welcomeEmailSubject;

                        //var replacementElements = new List<KeyValuePair<string, string>>();
                        //replacementElements.Add(new KeyValuePair<string, string>("EMAILADDRESS", user.EmailAddress));
                        //replacementElements.Add(new KeyValuePair<string, string>("LINK_ACTIVATION", String.Format(_activationUrl, user.ConfirmationToken)));

                        //_emailService.SendEmail(user.EmailAddress, emailSubject, _templateName, replacementElements);

                    }

                    _logger.Log(LogLevel.Info, string.Format("Processing Registered User for {0}. Finished.", user.UserId));
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Fatal, string.Format("Unhandled Exception Processing Registered User. Exception: {0}", ex.Message));

                    var innerException = ex.InnerException;

                    while (innerException != null)
                    {
                        _logger.Log(LogLevel.Fatal, string.Format("Unhandled Exception Processing Registered User. Inner Exception: {0}", innerException.Message));

                        innerException = innerException.InnerException;
                    }

                }
            }

        }
    }
}
