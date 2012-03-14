using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using SocialPayments.DataLayer;
using System.ServiceModel.Activation;
using SocialPayments.Services.DataContracts.BetaSignUp;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BetaSignupService : IBetaSignupService
    {
        private Context _ctx = new Context();
        
        public DataContracts.BetaSignUp.BetaSignupResponse AddBetaSignUp(BetaSignUpRequest request)
        {
            var signUp = _ctx.BetaSignUps.FirstOrDefault(b => b.EmailAddress == request.EmailAddress);

            if(signUp != null)
            {
                return new DataContracts.BetaSignUp.BetaSignupResponse() {
                    Success = false,
                    Message = String.Format("Thanks for your intereset, but {0} is already signed up.", request.EmailAddress)
                };
            }

            var newSignUp = _ctx.BetaSignUps.Add(new Domain.BetaSignup()
            {
                Id = Guid.NewGuid(),
                EmailAddress = request.EmailAddress,
                CreateDate = System.DateTime.Now
            });

            try
            {
                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                return new DataContracts.BetaSignUp.BetaSignupResponse()
                {
                    Success = false,
                    Message = String.Format("Error occurred adding beta user. {0}", ex.Message)
                };
            }

            return new DataContracts.BetaSignUp.BetaSignupResponse() {
                Id = newSignUp.Id,
                EmailAddress = newSignUp.EmailAddress,
                Success = true,
                Message = String.Format("Thanks for your interest.  We will contact you shortly.", newSignUp.EmailAddress)
            };
        }
    }
}