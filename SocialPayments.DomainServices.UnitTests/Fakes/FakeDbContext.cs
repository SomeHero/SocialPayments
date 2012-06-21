using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using System.Data.Entity;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeDbContext : IDbContext
    {
        private IDbSet<Application> _applications;
        private IDbSet<User> _users;
        private IDbSet<UserAttribute> _userAttributes;
        private IDbSet<Role> _roles;
        private IDbSet<Message> _messages;
        private IDbSet<Payment> _payments;
        private IDbSet<PaymentAccount> _paymentAccounts;
        private IDbSet<BatchFile> _batchFiles;
        private IDbSet<Calendar> _calendars;
        private IDbSet<EmailLog> _emailLog;
        private IDbSet<SMSLog> _smsLog;
        private IDbSet<Transaction> _transactions;
        private IDbSet<TransactionBatch> _transactionBatch;
        private IDbSet<BetaSignup> _betaSignUps;
        private IDbSet<MobileNumberSignUpKey> _mobileNumberSignUpKey;
        private IDbSet<MECode> _meCodes;
        private IDbSet<PaymentAccountVerification> _paymentAccountVerifications;
        private IDbSet<SecurityQuestion> _securityQuestions;

        public FakeDbContext()
        {
            _applications = new FakeApplicationSet();
            _batchFiles = new FakeBatchFileSet();
            _betaSignUps = new FakeBetaSignUpSet();
            _calendars = new FakeCalendarSet();
            _messages = new FakeMessageSet();
            _userAttributes = new FakeUserAttributeSet();
            _users = new FakeUserSet();
            _smsLog = new FakeSMSLogSet();
            _emailLog = new FakeEmailLogSet();
            _transactions = new FakeTransactionSet();
            _transactionBatch = new FakeTransactionBatchSet();
            _meCodes = new FakeMECodeSet();
            _roles = new FakeRoleSet();
            _paymentAccounts = new FakePaymentAccountSet();
            _paymentAccountVerifications = new FakePaymentAccountVerificationSet();
        }
        public IDbSet<Application> Applications
        {
            get { return _applications; }
            set { _applications = value; }
        }
        public IDbSet<User> Users
        {
            get { return _users; }
            set { _users = value; }
        }
        public IDbSet<UserAttribute> UserAttributes
        {
            get { return _userAttributes; }
            set { _userAttributes = value; }
        }
        public IDbSet<Role> Roles
        {
            get { return _roles; }
            set { _roles = value; }
        }
        public IDbSet<Message> Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }
        public IDbSet<Payment> Payments
        {
            get { return _payments; }
            set { _payments = value; }
        }
        public IDbSet<PaymentAccount> PaymentAccounts
        {
            get { return _paymentAccounts; }
            set { _paymentAccounts = value; }
        }
        public IDbSet<BatchFile> BatchFiles
        {
            get { return _batchFiles; }
            set { _batchFiles = value; }
        }
        public IDbSet<Calendar> Calendars
        {
            get { return _calendars; }
            set { _calendars = value; }
        }
        public IDbSet<EmailLog> EmailLog
        {
            get { return _emailLog; }
            set { _emailLog = value; }
        }
        public IDbSet<SMSLog> SMSLog
        {
            get { return _smsLog; }
            set { _smsLog = value; }
        }
        public IDbSet<Transaction> Transactions
        {
            get { return _transactions; }
            set { _transactions = value; }
        }
        public IDbSet<TransactionBatch> TransactionBatches
        {
            get { return _transactionBatch; }
            set { _transactionBatch = value; }
        }
        public IDbSet<BetaSignup> BetaSignUps
        {
            get { return _betaSignUps; }
            set { _betaSignUps = value; }
        }
        public IDbSet<MobileNumberSignUpKey> MobileNumberSignUpKeys
        {
            get { return _mobileNumberSignUpKey; }
            set { _mobileNumberSignUpKey = value; }
        }
        public IDbSet<MECode> MECodes
        {
            get { return _meCodes; }
            set { _meCodes = value; }
        }
        public IDbSet<PaymentAccountVerification> PaymentAccountVerifications
        {
            get { return _paymentAccountVerifications; }
            set { _paymentAccountVerifications = value; }
        }
        public IDbSet<SecurityQuestion> SecurityQuestions
        {
            get { return _securityQuestions; }
            set { _securityQuestions = value; }
        }
        public void SaveChanges()
        {
            //return 0;
            //do nothing
        }
    }
}
