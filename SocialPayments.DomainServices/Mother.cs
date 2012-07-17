using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.Services.MessageProcessors.UnitTest
{
    public static class Mother
    {
        public static Domain.Application CreateApplication(IDbContext context)
        {
            var application = context.Applications.Add(new Domain.Application()
            {
                ApiKey = Guid.NewGuid(),
                ApplicationName = "Test App",
                IsActive = true,
                Url = "http:\\test.paidthx.com"
            });

            return application;
        }
        public static Domain.User CreateSender(IDbContext context, Domain.Application application, string senderEmail,
            string senderMobileNumber) {

            var sender = context.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = application.ApiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = senderEmail,
                Limit = 100,
                MobileNumber = senderMobileNumber,
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Active,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            context.SaveChanges();

            return sender;

        }
        public static Domain.User CreateRecipient(IDbContext context, Domain.Application application, string recipientEmail,
            string recipientMobileNumber)
        {
            var recipient = context.Users.Add(new Domain.User()
            {
                Application = application,
                ApiKey = application.ApiKey,
                CreateDate = System.DateTime.Now,
                EmailAddress = recipientEmail,
                Limit = 100,
                MobileNumber = recipientMobileNumber,
                Password = "asdf",
                PaymentAccounts = new System.Collections.ObjectModel.Collection<Domain.PaymentAccount>(),
                IsConfirmed = true,
                SecurityPin = "1234",
                SetupPassword = true,
                SetupSecurityPin = true,
                UserStatus = Domain.UserStatus.Active,
                DeviceToken = "6b0bf548627aecffe1a87b3febf62c9f6eda50c35b6acce067a21b365dcc94b4"
            });

            context.SaveChanges();

            return recipient;
        }
        public static Domain.PaymentAccount CreatePaymentAccount(IDbContext context, Domain.User user)
        {
            var senderAccount = new Domain.PaymentAccount()
            {
                AccountNumber = "1234123412",
                AccountType = Domain.PaymentAccountType.Checking,
                IsActive = true,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                NameOnAccount = "Chris Magee",
                RoutingNumber = "053000219",
            };

            user.PaymentAccounts.Add(senderAccount);

            context.SaveChanges();

            return senderAccount;
        }
        public static Domain.Message CreateMessageWithUnknownRecipient(IDbContext context, Domain.Application application, 
            Domain.User sender, string senderUri, string recipientUri, Domain.MessageType messageType)
        {
            var message = context.Messages.Add(new Domain.Message()
            {
                Amount = 1,
                Application = application,
                ApiKey = application.ApiKey,
                Comments = "Test Payment",
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                Status = Domain.PaystreamMessageStatus.Processing,
                MessageType = messageType,
                RecipientUri = recipientUri,
                Sender = sender,
                SenderId = sender.UserId,
                SenderAccountId = sender.PaymentAccounts[0].Id,
                SenderUri = senderUri,
                //Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>(),
            });

            context.SaveChanges();

            return message;
        }
        public static Domain.Message CreateMessageWithKnownRecipient(IDbContext context, Domain.Application application,
            Domain.User sender, Domain.User recipient, string senderUri, string recipientUri, Domain.MessageType messageType)
        {
            var message = context.Messages.Add(new Domain.Message()
            {
                Amount = 1,
                Application = application,
                ApiKey = application.ApiKey,
                Comments = "Test Payment",
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                Status = Domain.PaystreamMessageStatus.Processing,
                MessageType = messageType,
                RecipientUri = recipientUri,
                Sender = sender,
                Recipient = recipient,
                SenderId = sender.UserId,
                SenderAccountId = sender.PaymentAccounts[0].Id,
                SenderUri = senderUri,
                //Transactions = new System.Collections.ObjectModel.Collection<Domain.Transaction>()
            });

            context.SaveChanges();

            return message;
        }
    }
}
