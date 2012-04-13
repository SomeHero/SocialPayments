using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.DataLayer;
using NLog;
using SocialPayments.Services.ServiceContracts;
using SocialPayments.Services.DataContracts.Transaction;
using System.ServiceModel.Activation;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TransactionService : ITransactionService
    {
        DomainServices.PaymentService paymentService = new DomainServices.PaymentService();
        DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();
        DomainServices.UserService userService = new DomainServices.UserService();
        private DomainServices.SecurityService securityService = new DomainServices.SecurityService();
        private Context _ctx = new Context();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<TransactionResponse> GetTransactions(String id)
        {
            logger.Log(LogLevel.Info, String.Format("Getting Transactions"));

            var transactions = _ctx.Transactions
                .Where(t => t.FromAccount.UserId == new Guid(id))
                .OrderByDescending(t => t.CreateDate)
                .ToList<Domain.Transaction>();

            logger.Log(LogLevel.Info, transactions.Count());

            try
            {
                return transactions.Select(t => new TransactionResponse()
                {
                    ACHTransactionId = t.ACHTransactionId,
                    Amount = t.Amount,
                    CreateDate = t.CreateDate.ToString("MM/dd/yyyy hh:mm tt"),
                    //FromAccount = t.FromAccount,
                    PaymentChannel = t.PaymentChannelType.ToString(),
                    PaymentId = t.PaymentId,
                    StandardEntryClass = t.StandardEntryClass.ToString(),
                    LastUpdatedDate = t.LastUpdatedDate,
                    TransactionBatchId = t.TransactionBatchId.ToString(),
                    TransactionCategory = t.Category.ToString(),
                    TransactionType = t.Type.ToString(),
                    TransactionStatus = t.Status.ToString(),
                    TransactionId = t.Id,
                    TransationSentDate = t.SentDate
                }).ToList();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Debug, ex.Message);
                throw ex;

            }
        }
    }
}