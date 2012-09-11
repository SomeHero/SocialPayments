using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.DomainServices
{
    public class SecurityQuestionServices
    {
        public void AddSecurityQuestion(string userId, int questionId, string questionAnswer)
        {
            using (var ctx = new Context())
            {
                DomainServices.SecurityService securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService(ctx);

                Domain.User user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("The user {0} not found", userId));

                user.SecurityQuestionAnswer = questionAnswer;
                user.SecurityQuestionID = questionId;

                ctx.SaveChanges();
            }
        }
        public List<Domain.SecurityQuestion> GetSecurityQuestions()
        {
            using (var ctx = new Context())
            {
                return ctx.SecurityQuestions.Where(q => q.IsActive)
                    .ToList();
            }
        }
        public Domain.SecurityQuestion GetSecurityQuestion(int id)
        {
            using (var ctx = new Context())
            {
                return ctx.SecurityQuestions.FirstOrDefault(q => q.Id == id);
            }
        }
        public void SetupSecurityQuestion(string userId, int questionId, string questionAnswer)
        {
            using (var ctx = new Context())
            {
                DomainServices.SecurityService securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService(ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} is Invalid", userId));

                if (questionId < 0 || questionAnswer.Length == 0)
                    throw new BadRequestException(String.Format("Security Question Id and Answer Are Invalid"));

                    
                user.SecurityQuestionAnswer = questionAnswer;
                user.SecurityQuestionID = questionId;

            }
        }
        public bool ValidateSecurityQuestion(string userId, string questionAnswer)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} is Invalid", userId));

                if (user.SecurityQuestionID == null || String.IsNullOrEmpty(user.SecurityQuestionAnswer))
                    throw new CustomExceptions.BadRequestException(String.Format("No security question was setup for user {0}", userId));


                if (questionAnswer.Equals(securityService.Decrypt(user.SecurityQuestionAnswer), StringComparison.OrdinalIgnoreCase))
                {
                    user.IsLockedOut = false;
                    user.PinCodeFailuresSinceLastSuccess = 0;

                    _userService.UpdateUser(user);

                    return true;
                }

                return false;
            }
        }
    }
}
