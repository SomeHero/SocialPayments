using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using SocialPayments.Domain;
using NLog;
using SocialPayments.DataLayer.Interfaces;
using System.Collections.ObjectModel;

namespace SocialPayments.DataLayer
{
    public class Context : DbContext, IDbContext
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
      
        public IDbSet<Application> Applications { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<UserAttribute> UserAttributes { get; set; }
        public IDbSet<Role> Roles { get; set; }
        public IDbSet<Message> Messages { get; set; }
        public IDbSet<PaymentAccount> PaymentAccounts { get; set; }
        public IDbSet<BatchFile> BatchFiles { get; set; }
        public IDbSet<Calendar> Calendars { get; set; }
        public IDbSet<EmailLog> EmailLog { get; set; }
        public IDbSet<SMSLog> SMSLog { get; set; }
        public IDbSet<Transaction> Transactions { get; set; }
        public IDbSet<TransactionBatch> TransactionBatches { get; set; }
        public IDbSet<MobileDeviceAlias> MobileDeviceAliases { get; set; }
        public IDbSet<BetaSignup> BetaSignUps { get; set; }
        public IDbSet<MobileNumberSignUpKey> MobileNumberSignUpKeys { get; set; }
        public IDbSet<MECode> MECodes { get; set; }
        public IDbSet<PaymentAccountVerification> PaymentAccountVerifications { get; set; }

        public Context() : base("name=DataContext") { }

        public void SaveChanges()
        {
            base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            _logger.Log(LogLevel.Info, String.Format("Creating Model"));

            try
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Message>()
                    .HasRequired(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApiKey)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<Message>()
                    .HasRequired(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<Message>()
                    .HasOptional(m => m.Recipient)
                    .WithMany()
                    .HasForeignKey(m => m.RecipientId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<Transaction>()
                    .HasRequired(t => t.FromAccount)
                    .WithMany()
                    .HasForeignKey(t => t.FromAccountId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<Transaction>()
                     .HasRequired(t => t.User)
                     .WithMany()
                     .HasForeignKey(t => t.UserId)
                     .WillCascadeOnDelete(false);

                modelBuilder.Entity<User>()
                    .HasOptional(u => u.FacebookUser)
                    .WithOptionalDependent(f => f.User)
                    .Map(m => m.MapKey("FBUserId"));

                modelBuilder.Entity<MECode>()
                    .HasRequired(m => m.User)
                    .WithMany()
                    .HasForeignKey(m => m.UserId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<UserAttributeValue>()
                    .HasRequired(u => u.UserAttribute)
                    .WithMany()
                    .HasForeignKey(u => u.UserAttributeId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<UserAttributePermission>()
                    .HasRequired(u => u.UserAttribute)
                    .WithMany()
                    .HasForeignKey(u => u.UserAttributeId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<UserAttributePermission>()
                    .HasRequired(u => u.User)
                    .WithMany()
                    .HasForeignKey(u => u.UserId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<UserAttributePermission>()
                    .HasRequired(u => u.Application)
                    .WithMany()
                    .HasForeignKey(u => u.ApiKey)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<MobileNumberSignUpKey>()
                    .HasKey(k => k.SignUpKey);

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception creating model. {0}", ex.Message));

                throw ex;
            }
        }
    }

    public class MyInitializer :System.Data.Entity.CreateDatabaseIfNotExists<Context>
    {
        private SecurityService securityService = new SecurityService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
      
        protected override void Seed(Context context)
        {
            _logger.Log(LogLevel.Info, String.Format("Seeding Database"));

            //create unique constraints
            context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_UserName ON Users (UserName)");
            context.Database.ExecuteSqlCommand("CREATE UNIQUE NONCLUSTERED INDEX IX_EmailAddress ON Users (EmailAddress) where EmailAddress Is Not Null and EmailAddress != ''");
            context.Database.ExecuteSqlCommand("CREATE UNIQUE NONCLUSTERED INDEX IX_MobileNumber ON Users (MobileNumber) where MobileNumber Is Not Null and MobileNumber != ''");

            //context.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX IX_PayPoint ON PayPoints (URI)");

            var adminRole = context.Roles.Add(new Role()
            {
                RoleId = Guid.NewGuid(),
                Description = "Administrator",
                RoleName = "Administrator",
            });
            var firstNameUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "FirstName",
                Approved = true,
                IsActive = true
            });
            var lastNameUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "LastName",
                Approved = true,
                IsActive = true
            });
            var address1Attribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "Address1",
                Approved = true,
                IsActive = true
            });
            var address2Attribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "Address2",
                Approved = true,
                IsActive = true
            });
            var cityUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "City",
                Approved = true,
                IsActive = true
            });
            var stateUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "State",
                Approved = true,
                IsActive = true
            });
            var zipCodeUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "ZipCode",
                Approved = true,
                IsActive = true
            });
            var memberRole = context.Roles.Add(new Role()
            {
                RoleId = Guid.NewGuid(),
                Description = "Member",
                RoleName = "Member"
            });
            context.Applications.Add(new Application()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                ApplicationName = "MyApp",
                IsActive = true,
                Url = "myurl.com"
            });
            context.SaveChanges();

            context.Users.Add(new User()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                UserId = Guid.NewGuid(),
                EmailAddress = "test@gmail.com",
                MobileNumber = "8043555555",
                UserName = "test@gmail.com",
                Password = securityService.Encrypt("testuser"),
                SecurityPin = securityService.Encrypt("1111"),
                PaymentAccounts = new Collection<PaymentAccount>() {
                    new PaymentAccount() { 
                        Id=Guid.NewGuid(), 
                        AccountNumber = securityService.Encrypt("411111111111"), 
                        AccountType = PaymentAccountType.Checking, 
                        NameOnAccount= securityService.Encrypt("Test User"), 
                        RoutingNumber= securityService.Encrypt("053000219"),
                        CreateDate = System.DateTime.Now,
                        IsActive = true
                    }
                },
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Verified,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Roles = new Collection<Role>()
                {
                    memberRole
                }
            });

            context.Users.Add(new User()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                UserId = Guid.NewGuid(),
                EmailAddress = "admin@pdthx.me",
                MobileNumber = "",
                UserName = "sysadmin",
                Password = securityService.Encrypt("pdthx123"),
                SecurityPin = "",
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Verified,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Roles = new Collection<Role>()
                {
                    adminRole
                }
            });
            context.Users.Add(new User()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                UserId = Guid.NewGuid(),
                EmailAddress = "james@paidthx.com",
                MobileNumber = "",
                UserName = "james@paidthx.com",
                Password = securityService.Encrypt("james123"),
                SecurityPin = securityService.Encrypt("2589"),
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Verified,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Roles = new Collection<Role>()
                {
                    adminRole,
                    memberRole
                },
                UserAttributes = new Collection<UserAttributeValue>()
                {
                    new UserAttributeValue() {
                        id = Guid.NewGuid(),
                        UserAttributeId = firstNameUserAttribute.Id,
                        AttributeValue = "James"
                    },
                    new UserAttributeValue() {
                        id = Guid.NewGuid(),
                        UserAttributeId = lastNameUserAttribute.Id,
                        AttributeValue = "Rhodes"
                    }
                }
            });
            context.SaveChanges();

            var user = context.Users.FirstOrDefault(u => u.EmailAddress == "test@gmail.com");
            var application = context.Applications.FirstOrDefault(a => a.ApiKey == new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"));

            var transactionBatch = context.TransactionBatches.Add(new TransactionBatch()
                                                                      {
                                                                          Id = Guid.NewGuid(),
                                                                          CreateDate = System.DateTime.Now,
                                                                          IsClosed = false
                                                                      });

            var message = context.Messages.Add(new Message()
            {
                Id = Guid.NewGuid(),
                Amount = 1.00,
                ApiKey = application.ApiKey,
                Comments = "Test Payment Message",
                MessageStatus = MessageStatus.Submitted,
                MessageStatusValue = (int)MessageStatus.Submitted,
                MessageType = MessageType.Payment,
                MessageTypeValue = (int)MessageType.Payment,
                RecipientUri = "804-387-9693",
                Sender = user,
                SenderId = user.UserId,
                SenderUri = user.MobileNumber,
                SenderAccount = user.PaymentAccounts[0],
                SenderAccountId = user.PaymentAccounts[0].Id,
                CreateDate = System.DateTime.Now,
                Application = application,
            });
            
            message.Transactions = new Collection<Transaction>()
                                       {
                                           new Transaction()
                                               {
                                                   Amount = 20.55,
                                                   Category = TransactionCategory.Payment,
                                                   CreateDate = System.DateTime.Now,
                                                   FromAccountId = user.PaymentAccounts[0].Id,
                                                   Id = Guid.NewGuid(),
                                                   MessageId = message.Id,
                                                   TransactionBatchId = transactionBatch.Id,
                                                   Type = TransactionType.Withdrawal,
                                                   StandardEntryClass =  Domain.StandardEntryClass.Web,
                                                   PaymentChannelType = Domain.PaymentChannelType.Single,
                                                   UserId = user.UserId
                                               },
                                               new Transaction()
                                               {
                                                       Amount = 20.55,
                                                   Category = TransactionCategory.Payment,
                                                   CreateDate = System.DateTime.Now,
                                                   FromAccountId = user.PaymentAccounts[0].Id,
                                                   Id = Guid.NewGuid(),
                                                   MessageId = message.Id,
                                                   TransactionBatchId = transactionBatch.Id,
                                                   Type = TransactionType.Deposit,
                                                   StandardEntryClass =  Domain.StandardEntryClass.Web,
                                                   PaymentChannelType = Domain.PaymentChannelType.Single,
                                                    UserId = user.UserId
                                               }
                                       };

            var calendar = context.Calendars.Add(new Calendar()
            {
                Application = application,
                CalendarCode = "NACHAHolidayCalendar",
                CalendarType = CalendarType.ExcludeDays,
            });
            var startDateTime = System.DateTime.Now;
            var endDateTime = System.DateTime.Now.AddYears(2);

            for (DateTime date = startDateTime; date.Date <= endDateTime.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek.Equals(DayOfWeek.Saturday) || date.DayOfWeek.Equals(DayOfWeek.Sunday))
                    calendar.CalendarDates.Add(new CalendarDate()
                    {
                        SelectedDate = date
                    });
            }
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
