using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class MarketingServices
    {
        public void AddBetaSignUp(string emailAddress)
        {
            using (var ctx = new Context())
            {
                var emailService = new DomainServices.EmailService(ctx);

                var signUp = ctx.BetaSignUps.FirstOrDefault(b => b.EmailAddress == emailAddress);


                if (signUp != null)
                    throw new CustomExceptions.BadRequestException(String.Format("{0} is already signed up!", emailAddress));

                var newSignUp = ctx.BetaSignUps.Add(new Domain.BetaSignup()
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = emailAddress,
                    CreateDate = System.DateTime.Now
                });

                ctx.SaveChanges();


                //Send Email
                try
                {
                    emailService.SendEmail(emailAddress, "Thanks for signing up to be one of the first users of PaidThx!", "Website - Confirmation Sign Up BETA", new List<KeyValuePair<string, string>>());
                }
                catch (Exception ex)
                {
                    //log
                }
            }
        }
    }
}
