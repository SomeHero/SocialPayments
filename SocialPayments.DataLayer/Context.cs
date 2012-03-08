using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using SocialPayments.Domain;

namespace SocialPayments.DataLayer
{
    public class Context : DbContext
    {
        public DbSet<Application> Applications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentAccount> PaymentAccounts { get; set; }
        public DbSet<BatchFile> BatchFiles { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<EmailLog> EmailLog { get; set; }
        public DbSet<SMSLog> SMSLog { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionBatch> TransactionBatches { get; set; }
        public DbSet<MobileDeviceAlias> MobileDeviceAliases { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }

        public Context() : base("name=DataContext") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.FromAccount)
                .WithMany()
                .HasForeignKey(p => p.FromAccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Payment>()
                .HasOptional(p => p.ToAccount)
                .WithMany()
                .HasForeignKey(p => p.ToAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PaymentRequest>()
                .HasRequired(r => r.Requestor)
                .WithMany()
                .HasForeignKey(r => r.RequestorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PaymentRequest>()
    .HasOptional(r => r.Recipient)
    .WithMany()
    .HasForeignKey(r => r.RecipientId)
    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transaction>()
                .HasRequired(t => t.FromAccount)
                .WithMany()
                .WillCascadeOnDelete(false);
        }

    }

    public class MyInitializer :System.Data.Entity.CreateDatabaseIfNotExists<Context>
    {
        private SecurityService securityService = new SecurityService();
       
        protected override void Seed(Context context)
        {
            var adminRole = context.Roles.Add(new Role()
            {
                RoleId = Guid.NewGuid(),
                Description = "Administrator",
                RoleName = "Administrator",
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
                MobileNumber = "804-355-5555",
                UserName = "8043555555",
                Password = securityService.Encrypt("testuser"),
                SecurityPin = securityService.Encrypt("1111"),
                PaymentAccounts = new List<PaymentAccount>() {
                    new PaymentAccount() { Id=Guid.NewGuid(), AccountNumber = securityService.Encrypt("411111111111"), AccountType = PaymentAccountType.Checking, 
                        NameOnAccount= securityService.Encrypt("Test User"), RoutingNumber= securityService.Encrypt("053000219") }
                },
                IsLockedOut = false,
                CreateDate = System.DateTime.Now,
                LastLoggedIn = System.DateTime.Now,
                UserStatus = UserStatus.Verified,
                IsConfirmed = true,
                RegistrationMethod = UserRegistrationMethod.Test,
                Roles = new List<Role>()
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
                Roles = new List<Role>()
                {
                    adminRole
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
            var payment = context.Payments.Add(new Payment()
            {
                Id = Guid.NewGuid(),
                Application = application,
                FromAccount = user.PaymentAccounts[0],
                FromMobileNumber = "804-387-9693",
                PaymentAmount = 20.55,
                PaymentDate = System.DateTime.Now,
                PaymentStatus = PaymentStatus.Submitted,
                PaymentChannelType = Domain.PaymentChannelType.Single,
                StandardEntryClass = Domain.StandardEntryClass.Web,
                ToMobileNumber = "615-517-8859",
                Comments = "Test Payment",
                CreateDate = System.DateTime.Now,
            });
            payment.Transactions = new List<Transaction>()
                                       {
                                           new Transaction()
                                               {
                                                   Amount = 20.55,
                                                   Category = TransactionCategory.Payment,
                                                   CreateDate = System.DateTime.Now,
                                                   FromAccountId = user.PaymentAccounts[0].Id,
                                                   Id = Guid.NewGuid(),
                                                   PaymentId = payment.Id,
                                                   TransactionBatchId = transactionBatch.Id,
                                                   Type = TransactionType.Withdrawal,
                                                   StandardEntryClass =  payment.StandardEntryClass,
                                                   PaymentChannelType = payment.PaymentChannelType
                                               },
                                               new Transaction()
                                               {
                                                       Amount = 20.55,
                                                   Category = TransactionCategory.Payment,
                                                   CreateDate = System.DateTime.Now,
                                                   FromAccountId = user.PaymentAccounts[0].Id,
                                                   Id = Guid.NewGuid(),
                                                   PaymentId = payment.Id,
                                                   TransactionBatchId = transactionBatch.Id,
                                                   Type = TransactionType.Deposit,
                                                   StandardEntryClass =  payment.StandardEntryClass,
                                                   PaymentChannelType = payment.PaymentChannelType
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
