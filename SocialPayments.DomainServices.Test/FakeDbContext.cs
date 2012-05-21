using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using System.Data.Entity;
using SocialPayments.Domain.Interfaces;

namespace SocialPayments.DomainServices.Test
{
    public class FakeDbContext : IDbContext
    {

        private IDbSet<IApplication> _applications;
        private DbSet<IUser> _users;
        private DbSet<IUserAttribute> _userAttributes;
        private DbSet<IRole> _roles;
        private DbSet<IMessage> _messages;
        private DbSet<IPaymentAccount> _paymentAccounts;
        private DbSet<IBatchFile> _batchFiles;
        private DbSet<ICalendar> _calendars;
        private DbSet<IEmailLog> _emailLog;
        private DbSet<ISMSLog> _smsLog;
        private DbSet<ITransaction> _transactions;
        private DbSet<ITransactionBatch> _transactionBatch;
        private DbSet<IBetaSignup> _betaSignUps;
        private DbSet<IMobileNumberSignUpKey> _mobileNumberSignUpKey;

        public FakeDbContext()
        {
            _applications = new List<IApplication>();
        }
        public DbSet<IApplication> Applications
        {
            get { return _applications; }
            set { _applications = value;  }
        }
        public DbSet<Domain.Interfaces.IUser> Users
        {
            get { return _users; }
            set { _users = value; }
        }
        public DbSet<IUserAttribute> UserAttributes
        {
            get { return _userAttributes; }
            set { _userAttributes = value; }
        }
        public DbSet<IRole> Roles
        {
            get { return _roles; }
            set { _roles = value; }
        }
        public DbSet<IMessage> Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }
        public DbSet<IPaymentAccount> PaymentAccounts
        {
            get { return _paymentAccounts; }
            set { _paymentAccounts = value; }
        }
        public DbSet<Domain.Interfaces.IBatchFile> BatchFiles
        {
            get { return _batchFiles; }
            set { _batchFiles = value; }
        }
        public DbSet<Domain.Interfaces.ICalendar> Calendars
        {
            get { return _calendars; }
            set { _calendars = value; }
        }
        public DbSet<Domain.Interfaces.IEmailLog> EmailLog
        {
            get { return _emailLog; }
            set { _emailLog = value; }
        }
        public DbSet<Domain.Interfaces.ISMSLog> SMSLog
        {
            get { return _smsLog; }
            set { _smsLog = value; }
        }
        public DbSet<ITransaction> Transactions
        {
            get { return _transactions; }
            set { _transactions = value; }
        }
        public DbSet<Domain.Interfaces.ITransactionBatch> TransactionBatches
        {
            get { return _transactionBatch; }
            set { _transactionBatch = value; }
        }
        public DbSet<Domain.Interfaces.IBetaSignup> BetaSignUps
        {
            get { return _betaSignUps; }
            set { _betaSignUps = value; }
        }
        public DbSet<Domain.Interfaces.IMobileNumberSignUpKey> MobileNumberSignUpKeys
        {
            get { return _mobileNumberSignUpKey; }
            set { _mobileNumberSignUpKey = value; }
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}
