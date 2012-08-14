using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Mobile_PaidThx.Models
{
    public class ProfileModels
    {
        public ProfileModels()
        {
            TransactionReceipts = new List<PaystreamModels.PaymentModel>();
            PaymentAccountsList = new ListPaymentAccountModel();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SenderName { get; set; }
        public string AccountType { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public PaymentModel Payment { get; set; }
        public List<PaystreamModels.PaymentModel> TransactionReceipts { get; set; }
        public ListPaymentAccountModel PaymentAccountsList { get; set; }

    }
    public class OrganizationModels
    {
        public class Organizations
        {
            public List<OrganizationModel> NonProfits { get; set; }
            public List<OrganizationModel> PublicDirectories { get; set; }
        }
        public class OrganizationModel
        {
            public String Name { get; set; }
            public String Slogan { get; set; }
            public Boolean HasInfo { get; set; }
            public String ImageUri { get; set; }
            public String Id { get; set; }
        }
    }
    public class BankAccountModel
    {
        public string BankName { get; set; }
        public string BankIconURL { get; set; }
        public string PaymentAccountId { get; set; }
        public string Nickname { get; set; }
        public string NameOnAccount { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }

    }
    public class UpdateProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SenderName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class CompleteProfileModel
    {
        public String SenderName { get; set; }
        public PaymentModel Payment { get; set; }
    }
    public class SendMoneyModel
    {
        public String RecipientUri { get; set; }
        public Double Amount { get; set; }
        public String Comments { get; set; }
        public String Pincode { get; set; }
    }
    public class DonateMoneyModel
    {
        public String Organization { get; set; }
        public String OrganizationId { get; set; }
        public Double Amount { get; set; }
        public String PledgerUri { get; set; }
        public String Comments { get; set; }
        public String Pincode { get; set; }
    }
    public class RequestMoneyModel
    {
        public String RecipientUri { get; set; }
        public Double Amount { get; set; }
        public String Comments { get; set; }
        public String Pincode { get; set; }
    }

    public class AddContactModel
    {
        public String reference1 { get; set; }
        public String reference2 { get; set; }
    }

    public enum TransactionType
    {
        Deposit = 1,
        Withdrawal = 2
    }
    public enum TransactionStatus
    {
        Submitted = 0,
        Pending = 1,
        Complete = 2,
        Cancelled = 3,
        Returned = 4
    }
    public enum MessageType
    {
        PaymentRequest = 0,
        Payment = 1
    }
    [Serializable]
    public class PaymentAttributes
    {
        public string OtherUri { get; set; }
        public bool Debits { get; set; }
        public bool Credits { get; set; }
        public bool Pending { get; set; }
        public bool Complete { get; set; }
    }

}