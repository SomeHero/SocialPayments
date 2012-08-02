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
        public IDbSet<Merchant> Merchants { get; set; }
        public IDbSet<PasswordResetAttempt> PasswordResetAttempts { get; set; }
        public IDbSet<UserPayPointVerification> UserPayPointVerifications { get; set; }


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

    public class MyInitializer : System.Data.Entity.CreateDatabaseIfNotExists<Context>
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
                IsActive = true,
                Points = 20
            });
            var lastNameUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "LastName",
                Approved = true,
                IsActive = true,
                Points = 20
            });
            var address1Attribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "Address1",
                Approved = true,
                IsActive = true,
                Points = 5
            });
            var address2Attribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "Address2",
                Approved = true,
                IsActive = true,
                Points = 0
            });
            var cityUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "City",
                Approved = true,
                IsActive = true,
                Points = 10
            });
            var stateUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "State",
                Approved = true,
                IsActive = true,
                Points = 10
            });
            var zipCodeUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "ZipCode",
                Approved = true,
                IsActive = true,
                Points = 20
            });
            var phoneUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "phoneUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 50
            });
            var emailUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "emailUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 20
            });
            var faceBookUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "facebookUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 50
            });
            var twitterUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "twitterUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 50
            });
            var aboutMeUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "aboutMeUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 10
            });
            var makePublicUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "makePublicUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 5
            });
            var photoIdUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "photoIdUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 50
            });
            var ssnUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "ssnUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 50
            });
            var birthDayUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "birthdayUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 20
            });
            var incomeUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "incomeUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 20
            });
            var creditScoreUserAttribute = context.UserAttributes.Add(new UserAttribute()
            {
                Id = Guid.NewGuid(),
                AttributeName = "creditScoreUserAttribute",
                Approved = true,
                IsActive = true,
                Points = 20
            });
            var memberRole = context.Roles.Add(new Role()
            {
                RoleId = Guid.NewGuid(),
                Description = "Member",
                RoleName = "Member",
            });
            var application = context.Applications.Add(new Application()
            {
                ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                ApplicationName = "MyApp",
                IsActive = true,
                Url = "myurl.com",
                CreateDate = System.DateTime.Now,
                ConfigurationValues = new Collection<ApplicationConfiguration>()
                {
                    new ApplicationConfiguration {
                        Id = Guid.NewGuid(),
                        ConfigurationKey = "UpperLimit",
                        ConfigurationType = "1",
                        ConfigurationValue = "5000"
                    }
                }
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
                        SortOrder = 1,
                        UserAttribute = firstNameUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Last Name",
                        SortOrder = 2,
                        UserAttribute = lastNameUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Phone",
                        SortOrder = 3,
                        UserAttribute = phoneUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Email",
                        SortOrder = 4,
                        UserAttribute = emailUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Facebook",
                        SortOrder = 5,
                        UserAttribute = faceBookUserAttribute,
                        ProfileItemType = ProfileItemType.SocialAccount
                    },
                    new ProfileItem() {
                        Label = "Twitter",
                        SortOrder = 6,
                        UserAttribute = twitterUserAttribute,
                        ProfileItemType = ProfileItemType.SocialAccount
                    },
                    new ProfileItem() {
                        Label = "About Me",
                        SortOrder = 7,
                        UserAttribute = aboutMeUserAttribute,
                        ProfileItemType = ProfileItemType.LongText
                    },
                    new ProfileItem() {
                        Label = "Make Public",
                        SortOrder = 8,
                        UserAttribute = makePublicUserAttribute,
                        ProfileItemType = ProfileItemType.Switch
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
                        SortOrder = 1,
                        UserAttribute = address1Attribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "City",
                        SortOrder = 2,
                        UserAttribute = cityUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "State",
                        SortOrder = 3,
                        UserAttribute = stateUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Zip",
                        SortOrder = 4,
                        UserAttribute = zipCodeUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Photo ID",
                        SortOrder = 5,
                        UserAttribute = photoIdUserAttribute,
                        ProfileItemType = ProfileItemType.ImageCapture
                    },
                    new ProfileItem() {
                        Label = "SSN",
                        SortOrder = 6,
                        UserAttribute = ssnUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Birthday",
                        SortOrder = 7,
                        UserAttribute = birthDayUserAttribute,
                        ProfileItemType = ProfileItemType.ShortText
                    },
                    new ProfileItem() {
                        Label = "Income",
                        SortOrder = 8,
                        UserAttribute = incomeUserAttribute,
                        ProfileItemType = ProfileItemType.Picker
                    },
                    new ProfileItem() {
                        Label = "Credit Score",
                        SortOrder = 9,
                        UserAttribute = creditScoreUserAttribute,
                        ProfileItemType = ProfileItemType.Picker
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
                Question = "Last 4 digits of drivers license",
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
                UserType = UserType.Individual
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
                },
                UserType = UserType.Individual
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
                        URI = "james@paidthx.com",
                        VerifiedDate = System.DateTime.Now,
                        Verified = true,
                        VerificationCode = "1234",
                        VerificationLink = ""
                    },
                     new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = phonePayPointType,
                        URI = "8043879693",
                        VerifiedDate = System.DateTime.Now,
                        Verified = true,
                        VerificationCode = "1234",
                        VerificationLink = ""
                    },
                     new UserPayPoint() {
                        CreateDate = System.DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsActive = true,
                        Type = meCodePayPoint,
                        URI = "$jamesrhodes",
                        VerifiedDate = System.DateTime.Now,
                        Verified = true,
                        VerificationCode = "1234",
                        VerificationLink = ""
                    }
                },
                UserType = UserType.Individual
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
                UserType = UserType.Individual
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
                Status = PaystreamMessageStatus.ProcessingPayment,
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
                    HoldDays = 0,
                    ScheduledProcessingDate = System.DateTime.Now,
                    //PaymentVerificationLevel = PaymentVerificationLevel.Verified
                };

            


            var acceptRequest = context.Messages.Add(new Message()
            {
                Id = Guid.NewGuid(),
                Amount = 1.00,
                ApiKey = application.ApiKey,
                Comments = "Test Payment Message",
                Status = PaystreamMessageStatus.PendingRequest,
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
                Status = PaystreamMessageStatus.PendingRequest,
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
                Status = PaystreamMessageStatus.ProcessingPayment,
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
                HoldDays = 0,
                ScheduledProcessingDate = System.DateTime.Now,
                //PaymentVerificationLevel = PaymentVerificationLevel.Verified
            };

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
                    Status = PaystreamMessageStatus.NotifiedPayment,
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
                        HoldDays = 0,
                        ScheduledProcessingDate = System.DateTime.Now,
                       // PaymentVerificationLevel = PaymentVerificationLevel.Verified
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
                    Status = PaystreamMessageStatus.NotifiedPayment,
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
                        HoldDays = 0,
                        ScheduledProcessingDate = System.DateTime.Now,
                       // PaymentVerificationLevel = PaymentVerificationLevel.Verified
                    }
                });


            }

            var merchant1 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Richmond Road Runners",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "rrr@paidthx.me",
                    UserName = "rrr@paidthx.me",
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
                    OrganizationName = "Richmond Road Runners",
                    UserType = UserType.NonProfit
                },
                MerchantType = MerchantType.Regular

            });

            var merchant2 = context.Merchants.Add(new Merchant() {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Richmond Sports League",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "ricsports@paidthx.me",
                    UserName = "ricsports@paidthx.me",
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
                    }
                },
                MerchantType = MerchantType.Regular
            });
            var merchant3 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Richmond Beard League",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "beardleague@paidthx.me",
                    UserName = "beardleague@paidthx.me",
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
                    }
                },
                MerchantType = MerchantType.Regular
            });
            var merchant4 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Hostelling International USA",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "hostelling@paidthx.me",
                    UserName = "hostelling@paidthx.me",
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
                    ImageUrl = "http://memberimages.paidthx.com/org_hostelling.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant5 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "American Cancer Society",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "acs@pdthx.me",
                    UserName = "acs@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_acs.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant6 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "American Diabetes Association",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "ada@pdthx.me",
                    UserName = "ada@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_americandiabetes.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant7 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "American Heart Association",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "aha@pdthx.me",
                    UserName = "aha@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_americanheart.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant8 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Beta Theta Pi",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "betas@pdthx.me",
                    UserName = "betas@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_beta.png",
                },
                MerchantType = MerchantType.Regular,

            });
            var merchant9 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Boy Scouts of America",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "boyscouts@pdthx.me",
                    UserName = "boyscouts@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_bsa.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant10 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Central Virginia Soccer Association",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "cvsa@pdthx.me",
                    UserName = "cvsa@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_cvsa.png",
                },
                MerchantType = MerchantType.Regular,

            });
            var merchant11 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Childsavers",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "childsavers@pdthx.me",
                    UserName = "childsavers@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_childsavers.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant12 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Holy Redeemer by the Sea",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "obxparish@pdthx.me",
                    UserName = "obxparish@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_obxparish.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant13 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Goodwill of Greater Washington",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "org_goodwill@pdthx.me",
                    UserName = "org_goodwill@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_goodwill.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant14 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "March of Dimes Foundation",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "marchofdimes@pdthx.me",
                    UserName = "marchofdimes@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_marchofdimes.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant15 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Nature Conservancy, Inc",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "natureconservancy@pdthx.me",
                    UserName = "natureconservancy@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_natureconservancy.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant16 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "River City Sports and Social Club",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "rivercitysports@pdthx.me",
                    UserName = "rivercitysports@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_rivercitysports.png",
                },
                MerchantType = MerchantType.Regular,

            });
            var merchant17 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Special Olympics",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "specialolympics@pdthx.me",
                    UserName = "specialolympics@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_specialolympics.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant18 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Susan G. Komen for the Cure",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "susangkomen@pdthx.me",
                    UserName = "susangkomen@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_teach4amer.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant19 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "The Rotary Foundation of Rotary International",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "rotaryfoundation@pdthx.me",
                    UserName = "rotaryfoundation@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_rotary.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant20 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "U-Turn",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "uturn@pdthx.me",
                    UserName = "uturn@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_uturn.png",
                },
                MerchantType = MerchantType.NonProfit,

            });

            var merchant22 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "World Vision",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "worldvision@pdthx.me",
                    UserName = "worldvision@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_worldvision.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant23 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "World Wildlife Fund",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "wwf@pdthx.me",
                    UserName = "wwf@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_wwf.png",
                },
                MerchantType = MerchantType.NonProfit,

            });
            var merchant24 = context.Merchants.Add(new Merchant()
            {
                Id = Guid.NewGuid(),
                CreateDate = System.DateTime.Now,
                Name = "Beta Tau Alpha",
                User = new User()
                {
                    ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"),
                    UserId = Guid.NewGuid(),
                    EmailAddress = "zta@pdthx.me",
                    UserName = "zta@pdthx.me",
                    Password = securityService.Encrypt("pdthx123"),
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
                    ImageUrl = "http://memberimages.paidthx.com/org_zta.png",
                },
                MerchantType = MerchantType.Regular,

            });
            context.SaveChanges();

            merchant1.User.PreferredReceiveAccount = merchant1.User.PaymentAccounts[0];
            merchant1.User.PreferredSendAccount = merchant1.User.PaymentAccounts[0];

            merchant2.User.PreferredReceiveAccount = merchant2.User.PaymentAccounts[0];
            merchant2.User.PreferredSendAccount = merchant2.User.PaymentAccounts[0];

            merchant3.User.PreferredReceiveAccount = merchant3.User.PaymentAccounts[0];
            merchant3.User.PreferredSendAccount = merchant3.User.PaymentAccounts[0];

            merchant4.User.PreferredReceiveAccount = merchant4.User.PaymentAccounts[0];
            merchant4.User.PreferredSendAccount = merchant4.User.PaymentAccounts[0];

            merchant5.User.PreferredReceiveAccount = merchant5.User.PaymentAccounts[0];
            merchant5.User.PreferredSendAccount = merchant5.User.PaymentAccounts[0];

            merchant6.User.PreferredReceiveAccount = merchant6.User.PaymentAccounts[0];
            merchant6.User.PreferredSendAccount = merchant6.User.PaymentAccounts[0];

            merchant7.User.PreferredReceiveAccount = merchant7.User.PaymentAccounts[0];
            merchant7.User.PreferredSendAccount = merchant7.User.PaymentAccounts[0];

            merchant8.User.PreferredReceiveAccount = merchant8.User.PaymentAccounts[0];
            merchant8.User.PreferredSendAccount = merchant8.User.PaymentAccounts[0];

            merchant9.User.PreferredReceiveAccount = merchant9.User.PaymentAccounts[0];
            merchant9.User.PreferredSendAccount = merchant9.User.PaymentAccounts[0];

            merchant10.User.PreferredReceiveAccount = merchant10.User.PaymentAccounts[0];
            merchant10.User.PreferredSendAccount = merchant10.User.PaymentAccounts[0];

            merchant11.User.PreferredReceiveAccount = merchant11.User.PaymentAccounts[0];
            merchant11.User.PreferredSendAccount = merchant11.User.PaymentAccounts[0];

            merchant12.User.PreferredReceiveAccount = merchant12.User.PaymentAccounts[0];
            merchant12.User.PreferredSendAccount = merchant12.User.PaymentAccounts[0];

            merchant13.User.PreferredReceiveAccount = merchant13.User.PaymentAccounts[0];
            merchant13.User.PreferredSendAccount = merchant13.User.PaymentAccounts[0];

            merchant14.User.PreferredReceiveAccount = merchant14.User.PaymentAccounts[0];
            merchant14.User.PreferredSendAccount = merchant14.User.PaymentAccounts[0];

            merchant15.User.PreferredReceiveAccount = merchant15.User.PaymentAccounts[0];
            merchant15.User.PreferredSendAccount = merchant15.User.PaymentAccounts[0];

            merchant16.User.PreferredReceiveAccount = merchant16.User.PaymentAccounts[0];
            merchant16.User.PreferredSendAccount = merchant16.User.PaymentAccounts[0];

            merchant17.User.PreferredReceiveAccount = merchant17.User.PaymentAccounts[0];
            merchant17.User.PreferredSendAccount = merchant17.User.PaymentAccounts[0];

            merchant18.User.PreferredReceiveAccount = merchant18.User.PaymentAccounts[0];
            merchant18.User.PreferredSendAccount = merchant18.User.PaymentAccounts[0];

            merchant19.User.PreferredReceiveAccount = merchant19.User.PaymentAccounts[0];
            merchant19.User.PreferredSendAccount = merchant19.User.PaymentAccounts[0];

            merchant20.User.PreferredReceiveAccount = merchant20.User.PaymentAccounts[0];
            merchant20.User.PreferredSendAccount = merchant20.User.PaymentAccounts[0];

            merchant22.User.PreferredReceiveAccount = merchant22.User.PaymentAccounts[0];
            merchant22.User.PreferredSendAccount = merchant22.User.PaymentAccounts[0];

            merchant23.User.PreferredReceiveAccount = merchant23.User.PaymentAccounts[0];
            merchant23.User.PreferredSendAccount = merchant23.User.PaymentAccounts[0];

            merchant24.User.PreferredReceiveAccount = merchant24.User.PaymentAccounts[0];
            merchant24.User.PreferredSendAccount = merchant24.User.PaymentAccounts[0];

            context.SaveChanges();
            

            base.Seed(context);
        }
    }
}
