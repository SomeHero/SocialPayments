using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using SocialPayments.Domain;
using NLog;
using System.Text.RegularExpressions;
using System.Data.Entity;
using SocialPayments.Services.UserProcessors.Interfaces;
using SocialPayments.Services.UserProcessors;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.Workflows.Users
{
    public class UserWorkflow
    {
        private readonly IDbContext _ctx;
        private static Logger logger;

        DomainServices.EmailService emailService = null;
        DomainServices.ApplicationService applicationServices = null;
        DomainServices.SMSLogService smsLogServices = null;
        DomainServices.SMSService smsService = null;
        DomainServices.TransactionBatchService transactionBatchService = null;

        public UserWorkflow()
        {
            _ctx = new Context();
            logger = LogManager.GetCurrentClassLogger();
            emailService = new DomainServices.EmailService(_ctx);
            applicationServices = new DomainServices.ApplicationService();
            smsLogServices = new SMSLogService(_ctx);
            smsService = new DomainServices.SMSService(applicationServices, smsLogServices, _ctx, logger);
            transactionBatchService = new DomainServices.TransactionBatchService(_ctx, logger);
        }
        public UserWorkflow(IDbContext context)
        {
            _ctx = context;
            logger = LogManager.GetCurrentClassLogger();
            emailService = new DomainServices.EmailService(_ctx);
            applicationServices = new DomainServices.ApplicationService();
            smsLogServices = new SMSLogService(_ctx);
            smsService = new DomainServices.SMSService(applicationServices, smsLogServices, _ctx, logger);
            transactionBatchService = new DomainServices.TransactionBatchService(_ctx, logger);
        }

        /// <summary>
        /// Process New PaidThx Member Registration
        /// </summary>
        /// <param name="id">unique identified of the user</param>
        public void Execute(string id)
        {
            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}, Starting.", id));

            Guid userId;
            Guid.TryParse(id, out userId);

            if (userId == null)
            {
                logger.Log(LogLevel.Error, string.Format("Error Processing New User Registration for {0}", id));
                throw new Exception(string.Format("Error Processing New User Registration for {0}", id));
            }

            logger.Log(LogLevel.Info, string.Format("Processing New User Registration for {0}. Retrieving User.", userId));
            var user = _ctx.Users
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                logger.Log(LogLevel.Error, string.Format("Error Processing New User Registration for {0}. Unable to find User.", userId));
            }

            switch (user.UserStatus)
            {
                case UserStatus.Submitted:
                    IUserProcessor processor = new SubmittedUserProcessor(_ctx, emailService, smsService);
                    processor.Process(user);

                    break;
                case UserStatus.Pending:

                    break;
            }
        }
    }
}
