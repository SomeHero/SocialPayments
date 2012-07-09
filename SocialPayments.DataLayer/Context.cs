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
        public IDbSet<Payment> Payments { get; set; }
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
        public IDbSet<SecurityQuestion> SecurityQuestions { get; set; }
        public IDbSet<NotificationType> NotificationTypes { get; set; }
        public IDbSet<PayPointType> PayPointTypes { get; set; }
        public IDbSet<UserPayPoint> UserPayPoints { get; set; }
        public IDbSet<UserNotification> UserNotificationConfigurations { get; set; }
        public IDbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }
        public IDbSet<UserConfiguration> UserConfigurations { get; set; }
        public IDbSet<SocialNetwork> SocialNetworks { get; set; }
        public IDbSet<UserSocialNetwork> UserSocialNetworks { get; set; }
        public IDbSet<ProfileSection> ProfileSections { get; set; }

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
                    .HasOptional(m => m.Recipient)
                    .WithMany()
                    .HasForeignKey(m => m.RecipientId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<Message>()
                    .HasOptional(m => m.Payment)
                    .WithOptionalDependent(p => p.Message)
                    .Map(m => m.MapKey("PaymentId"))
                    .WillCascadeOnDelete(false);
                    
                modelBuilder.Entity<Payment>()
                    .HasRequired(m => m.Application)
                    .WithMany()
                    .HasForeignKey(m => m.ApiKey)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<User>()
                    .HasOptional(u => u.FacebookUser)
                    .WithOptionalDependent(f => f.User)
                    .Map(m => m.MapKey("FBUserId"));

                modelBuilder.Entity<User>()
                    .HasOptional(m => m.SecurityQuestion)
                    .WithMany()
                    .HasForeignKey(u=> u.SecurityQuestionID)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<User>()
                    .HasOptional(m => m.PreferredSendAccount)
                    .WithMany()
                    .HasForeignKey(u => u.PreferredSendAccountId)
                    .WillCascadeOnDelete(false);

                modelBuilder.Entity<User>()
                    .HasOptional(m => m.PreferredReceiveAccount)
                    .WithMany()
                    .HasForeignKey(u => u.PreferredReceiveAccountId)
                    .WillCascadeOnDelete(false);

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
            context.Database.ExecuteSqlCommand("CREATE UNIQUE NONCLUSTERED INDEX IX_PayPointURI ON UserPayPoints (Uri)");

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
            var application = context.Applications.Add(new Application()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                ApplicationName = "MyApp",
                IsActive = true,
                Url = "myurl.com",
                CreateDate = System.DateTime.Now
            });
            context.SocialNetworks.Add(new SocialNetwork()
            {
                Id = Guid.NewGuid(),
                Name = "Facebook",
                Active = true
            });
            context.SocialNetworks.Add(new SocialNetwork()
            {
                Id = Guid.NewGuid(),
                Name = "Twitter",
                Active = true
            });
            context.SocialNetworks.Add(new SocialNetwork()
            {
                Id = Guid.NewGuid(),
                Name = "LinkedIn",
                Active = true
            });
            context.SocialNetworks.Add(new SocialNetwork()
            {
                Id = Guid.NewGuid(),
                Name = "Google +",
                Active = true
            });
            context.ProfileSections.Add(new ProfileSection()
            {
                SectionHeader = "",
                SortOrder = 1,
                ProfileItems = new Collection<ProfileItem>()
                {
                    new ProfileItem() {
                        Label = "First Name",
                        SortOrder = 1
                    },
                    new ProfileItem() {
                        Label = "Last Name",
                        SortOrder = 2
                    },
                    new ProfileItem() {
                        Label = "Phone",
                        SortOrder = 3
                    },
                    new ProfileItem() {
                        Label = "Email",
                        SortOrder = 4
                    },
                    new ProfileItem() {
                        Label = "Facebook",
                        SortOrder = 5
                    },
                    new ProfileItem() {
                        Label = "Twitter",
                        SortOrder = 6
                    },
                    new ProfileItem() {
                        Label = "About Me",
                        SortOrder = 7
                    },
                    new ProfileItem() {
                        Label = "Make Public",
                        SortOrder = 8
                    },
                }
            });
            context.ProfileSections.Add(new ProfileSection()
            {
                SectionHeader = "Secure Information (Never Public)",
                SortOrder = 2,
                ProfileItems = new Collection<ProfileItem>()
                {
                    new ProfileItem() {
                        Label = "Address",
                        SortOrder = 1
                    },
                    new ProfileItem() {
                        Label = "City",
                        SortOrder = 2
                    },
                    new ProfileItem() {
                        Label = "State  ",
                        SortOrder = 3
                    },
                    new ProfileItem() {
                        Label = "Zip",
                        SortOrder = 4
                    },
                    new ProfileItem() {
                        Label = "Photo ID",
                        SortOrder = 5
                    },
                    new ProfileItem() {
                        Label = "SSN",
                        SortOrder = 6
                    },
                    new ProfileItem() {
                        Label = "Birthday",
                        SortOrder = 7
                    },
                    new ProfileItem() {
                        Label = "Income",
                        SortOrder = 8
                    },
                    new ProfileItem() {
                        Label = "Credit Score",
                        SortOrder = 9
                    },
                }
            });
            var emailPayPointType = context.PayPointTypes.Add(new PayPointType()
            {
                Active = true,
                Id = 1,
                Name = "EmailAddress"
            });
            var phonePayPointType = context.PayPointTypes.Add(new PayPointType()
            {
                Active = true,
                Id = 2,
                Name = "Phone"
            });
            var facebookPayPoint = context.PayPointTypes.Add(new PayPointType()
            {
                Active = true,
                Id = 3,
                Name = "Facebook"
            });
            var meCodePayPoint = context.PayPointTypes.Add(new PayPointType()
            {
                Active = true,
                Id = 4,
                Name = "MeCode"
            });
            var twitterPayPoint = context.PayPointTypes.Add(new PayPointType()
            {
                Active = true,
                Id = 5,
                Name = "Twitter"
            });
            var smsNotificationType = context.NotificationTypes.Add(new NotificationType()
            {
                Active = true,
                Id = 1,
                Type = "SMS"
            });
            var emailNotificationType = context.NotificationTypes.Add(new NotificationType()
            {
                Active = true,
                Id = 2,
                Type = "Email"
            });
            var pushNotificationType = context.NotificationTypes.Add(new NotificationType()
            {
                Active = true,
                Id = 3,
                Type = "Push"
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 0,
                Question = "Childhood Nickname",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 1,
                Question = "Last 4 digis of drivers license",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 2,
                Question = "City you met your significant other",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 3,
                Question = "Street you lived on in 3rd grade",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 4,
                Question = "Oldest sibling's birth month and year",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 5,
                Question = "Childhood phone number including area code",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 6,
                Question = "Oldest cousin's first and last name",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 7,
                Question = "City your parents met",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 8,
                Question = "Last name of your 3rd grade teacher",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 9,
                Question = "Your first kiss was with...",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 10,
                Question = "Your childhood hero",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 11,
                Question = "Your dream job",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 12,
                Question = "School you attended for 6th grade",
                IsActive = true
            });
            context.SecurityQuestions.Add(new SecurityQuestion()
            {
                Id = 13,
                Question = "Oldest sibling's middle name",
                IsActive = true
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
                        Nickname = "Wachovia ****1111",
                        AccountNumber = securityService.Encrypt("411111111111"), 
                        AccountType = PaymentAccountType.Checking, 
                        NameOnAccount= securityService.Encrypt("Test User"), 
                        RoutingNumber= securityService.Encrypt("053000219"),
                        CreateDate = System.DateTime.Now,
                        IsActive = true,
                        BankIconURL = "http://images.PaidThx.com/BankIcons/bank.png",
                        BankName = "Wachovia"
                    }
                },
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Active,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Roles = new Collection<Role>()
                {
                    memberRole
                },
                PayPoints = new Collection<UserPayPoint>()
                {
                    new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = emailPayPointType,
                        URI = "test@gmail.com"
                    },
                     new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = phonePayPointType,
                        URI = "8043555555"
                    },
                },
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
                UserStatus = UserStatus.Active,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Roles = new Collection<Role>()
                {
                    adminRole
                }
            });
            var james = context.Users.Add(new User()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                UserId = Guid.NewGuid(),
                EmailAddress = "james@paidthx.com",
                MobileNumber = "8043879693",
                UserName = "james@paidthx.com",
                Password = securityService.Encrypt("james123"),
                SecurityPin = securityService.Encrypt("2589"),
                SetupPassword = true,
                SetupSecurityPin = true,
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Active,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Limit = 100,
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
                },
                FirstName = "James",
                LastName = "Rhodes",
                PaymentAccounts = new Collection<PaymentAccount>() {
                    new PaymentAccount() { 
                        Id=Guid.NewGuid(), 
                        Nickname = "BB&T ****1111",
                        AccountNumber = securityService.Encrypt("411111111111"), 
                        AccountType = PaymentAccountType.Checking, 
                        NameOnAccount= securityService.Encrypt("James Rhodes"), 
                        RoutingNumber= securityService.Encrypt("053000219"),
                        CreateDate = System.DateTime.Now,
                        IsActive = true,
                         BankIconURL = "http://images.PaidThx.com/BankIcons/bank.png",
                        BankName = "BB&T"
                    }
                },
                PayPoints = new Collection<UserPayPoint>()
                {
                    new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = emailPayPointType,
                        URI = "james@paidthx.com"
                    },
                     new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = phonePayPointType,
                        URI = "8043879693"
                    },
                     new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = meCodePayPoint,
                        URI = "$jamesrhodes"
                    }
                },
            });

            var sender = context.Users.Add(new User()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                UserId = Guid.NewGuid(),
                EmailAddress = "james@pdthx.me",
                MobileNumber = "8043550001",
                UserName = "james@pdthx.me",
                Password = securityService.Encrypt("james123"),
                SecurityPin = securityService.Encrypt("2589"),
                SetupPassword = true,
                SetupSecurityPin = true,
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Active,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Limit = 100,
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
                },
                PaymentAccounts = new Collection<PaymentAccount>() {
                    new PaymentAccount() { 
                        Id=Guid.NewGuid(), 
                        Nickname = "Wells Fargo ****9999",
                        AccountNumber = securityService.Encrypt("9999999999"), 
                        AccountType = PaymentAccountType.Checking, 
                        NameOnAccount= securityService.Encrypt("James Rhodes"), 
                        RoutingNumber= securityService.Encrypt("053000219"),
                        CreateDate = System.DateTime.Now,
                        IsActive = true,
                         BankIconURL = "http://images.PaidThx.com/BankIcons/bank.png",
                        BankName = "Wells Fargo"
                    }
                },
            });

            var meCode = context.MECodes.Add(new MECode()
            {
                ApprovedDate = System.DateTime.Now,
                CreateDate = System.DateTime.Now,
                Id = Guid.NewGuid(),
                IsActive = true,
                IsApproved = true,
                MeCode = "$therealjamesrhodes",
                User = sender
            });

           
            var transactionBatch = context.TransactionBatches.Add(new TransactionBatch()
                                                                      {
                                                                          Id = Guid.NewGuid(),
                                                                          CreateDate = System.DateTime.Now,
                                                                          IsClosed = false
                                                                      });

            context.SaveChanges();

            var sentMessage = context.Messages.Add(new Message()
            {
                Id = Guid.NewGuid(),
                Amount = 1.00,
                ApiKey = application.ApiKey,
                Comments = "Test Payment Message",
                Status = PaystreamMessageStatus.Processing,
                MessageType = MessageType.Payment,
                MessageTypeValue = (int)MessageType.Payment,
                Recipient = sender,
                RecipientUri = sender.MobileNumber,
                Sender = james,
                SenderUri = james.MobileNumber,
                SenderAccount = sender.PaymentAccounts[0],
                CreateDate = System.DateTime.Now,
                Application = application,
            });

            var sentPayment = sentMessage.Payment = new Payment()
            {
                    Id = Guid.NewGuid(),
                    Amount = sentMessage.Amount,
                    ApiKey = sentMessage.ApiKey,
                    Comments = sentMessage.Comments,
                    Message = sentMessage,
                    CreateDate = System.DateTime.Now,
                    PaymentStatus = PaymentStatus.Pending,
                    RecipientAccount = james.PaymentAccounts[0],
                    SenderAccount = sentMessage.Sender.PaymentAccounts[0],
                    Transactions = new Collection<Transaction>()
                };

            sentPayment.Transactions.Add(new Transaction()
            {
                ACHTransactionId = "",
                Amount = sentPayment.Amount,
                Category = TransactionCategory.Payment,
                CreateDate = System.DateTime.Now,
                FromAccount = sentPayment.SenderAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                TransactionBatch = transactionBatch,
                Type = TransactionType.Withdrawal,
                User = sentMessage.Sender,
                Payment = sentPayment,
                Status = TransactionStatus.Pending,
            });

            sentPayment.Transactions.Add(new Transaction()
            {
                ACHTransactionId = "",
                Amount = sentPayment.Amount,
                Category = TransactionCategory.Payment,
                CreateDate = System.DateTime.Now,
                FromAccount = sentPayment.RecipientAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                TransactionBatch = transactionBatch,
                Type = TransactionType.Deposit,
                User = sentMessage.Recipient,
                Payment = sentPayment,
                Status = TransactionStatus.Pending,
            });


            var acceptRequest = context.Messages.Add(new Message()
            {
                Id = Guid.NewGuid(),
                Amount = 1.00,
                ApiKey = application.ApiKey,
                Comments = "Test Payment Message",
                Status = PaystreamMessageStatus.Processing,
                MessageType = MessageType.PaymentRequest,
                Recipient = james,
                RecipientUri = james.MobileNumber,
                Sender = sender,
                SenderUri = sender.MobileNumber,
                SenderAccount = sender.PaymentAccounts[0],
                CreateDate = System.DateTime.Now,
                Application = application,
            });

            var sendRequest = context.Messages.Add(new Message()
            {
                Id = Guid.NewGuid(),
                Amount = 1.00,
                ApiKey = application.ApiKey,
                Comments = "Test Payment Message",
                Status = PaystreamMessageStatus.Processing,
                MessageType = MessageType.PaymentRequest,
                Recipient = sender,
                RecipientUri = sender.MobileNumber,
                Sender = james,
                SenderUri = james.MobileNumber,
                SenderAccount = sender.PaymentAccounts[0],
                CreateDate = System.DateTime.Now,
                Application = application,
            });

            var receivedMessage = context.Messages.Add(new Message()
            {
                Id = Guid.NewGuid(),
                Amount = 12.00,
                ApiKey = application.ApiKey,
                Comments = "thanks for the cheeseburger",
                Status = PaystreamMessageStatus.Processing,
                MessageType = MessageType.Payment,
                MessageTypeValue = (int)MessageType.Payment,
                Recipient = james,
                RecipientUri = james.MobileNumber,
                Sender = sender,
                SenderUri = sender.MobileNumber,
                SenderAccount = sender.PaymentAccounts[0],
                CreateDate = System.DateTime.Now,
                Application = application,
            });

            var receivedPayment = receivedMessage.Payment = new Payment()
            {
                Id = Guid.NewGuid(),
                Amount = receivedMessage.Amount,
                ApiKey = receivedMessage.ApiKey,
                Comments = receivedMessage.Comments,
                Message = receivedMessage,
                CreateDate = System.DateTime.Now,
                PaymentStatus = PaymentStatus.Pending,
                RecipientAccount = james.PaymentAccounts[0],
                SenderAccount = receivedMessage.Sender.PaymentAccounts[0],
                Transactions = new Collection<Transaction>()
            };

            receivedPayment.Transactions.Add(new Transaction()
            {
                ACHTransactionId = "",
                Amount = receivedPayment.Amount,
                Category = TransactionCategory.Payment,
                CreateDate = System.DateTime.Now,
                FromAccount = receivedPayment.SenderAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                TransactionBatch = transactionBatch,
                Type = TransactionType.Withdrawal,
                User = receivedMessage.Sender,
                Payment = receivedPayment,
                Status = TransactionStatus.Pending,
            });

            receivedPayment.Transactions.Add(new Transaction()
            {
                ACHTransactionId = "",
                Amount = receivedPayment.Amount,
                Category = TransactionCategory.Payment,
                CreateDate = System.DateTime.Now,
                FromAccount = receivedPayment.RecipientAccount,
                Id = Guid.NewGuid(),
                PaymentChannelType = PaymentChannelType.Single,
                StandardEntryClass = StandardEntryClass.Web,
                TransactionBatch = transactionBatch,
                Type = TransactionType.Deposit,
                User = receivedMessage.Recipient,
                Payment = receivedPayment,
                Status = TransactionStatus.Pending,
            });

            context.SaveChanges();

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

            for (var i = 0; i < 10; i++)
            {
                var amount = i;
                var comments = "Thanks for lunch.";

                context.Messages.Add(new Message()
                {
                    Id = Guid.NewGuid(),
                    Amount = amount,
                    ApiKey = application.ApiKey,
                    Comments = comments,
                    Status = PaystreamMessageStatus.Processing,
                    MessageType = MessageType.Payment,
                    MessageTypeValue = (int)MessageType.Payment,
                    RecipientUri = "804355000" + i,
                    Sender = james,
                    SenderUri = james.MobileNumber,
                    SenderAccount = james.PaymentAccounts[0],
                    CreateDate = System.DateTime.Now,
                    Application = application,
                    Payment = new Payment()
                    {
                        Amount = amount,
                        Application = application,
                        Comments = comments,
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        PaymentStatus = PaymentStatus.Pending,
                        SenderAccount = james.PaymentAccounts[0],
                        Transactions = new Collection<Transaction>()
                        {
                            new Transaction() {
                                Amount = amount,
                                Category = TransactionCategory.Payment,
                                CreateDate = System.DateTime.Now,
                                FromAccount = james.PaymentAccounts[0],
                                Id = Guid.NewGuid(),
                                PaymentChannelType = PaymentChannelType.Single,
                                StandardEntryClass = StandardEntryClass.Web,
                                Status = TransactionStatus.Complete,
                                Type = TransactionType.Withdrawal,
                                User =james
                            }
                        }
                    }
                });


            }


            for (var i = 10; i < 20; i++)
            {
                var amount = i;
                var comments = "Thanks for drinks.";

                context.Messages.Add(new Message()
                {
                    Id = Guid.NewGuid(),
                    Amount = amount,
                    ApiKey = application.ApiKey,
                    Comments = comments,
                    Status = PaystreamMessageStatus.Processing,
                    MessageType = MessageType.Payment,
                    MessageTypeValue = (int)MessageType.Payment,
                    RecipientUri = String.Format("james{0}@paidthx.com", i),
                    Sender = james,
                    SenderUri = james.MobileNumber,
                    SenderAccount = james.PaymentAccounts[0],
                    CreateDate = System.DateTime.Now,
                    Application = application,
                    Payment = new Payment()
                    {
                        Amount = amount,
                        Application = application,
                        Comments = comments,
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        PaymentStatus = PaymentStatus.Pending,
                        SenderAccount = james.PaymentAccounts[0],
                        Transactions = new Collection<Transaction>()
                        {
                            new Transaction() {
                                Amount = amount,
                                Category = TransactionCategory.Payment,
                                CreateDate = System.DateTime.Now,
                                FromAccount = james.PaymentAccounts[0],
                                Id = Guid.NewGuid(),
                                PaymentChannelType = PaymentChannelType.Single,
                                StandardEntryClass = StandardEntryClass.Web,
                                Status = TransactionStatus.Complete,
                                Type = TransactionType.Withdrawal,
                                User =james
                            }
                        }
                    }
                });


            }

            context.SaveChanges();
            

            base.Seed(context);
        }
    }
}
