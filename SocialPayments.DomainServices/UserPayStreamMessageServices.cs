using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class UserPayStreamMessageServices
    {
        public List<Domain.Message> GetPayStreamMessage(string userId)
        {
            using (var ctx = new Context())
            {
                DomainServices.MessageServices messageServices = new DomainServices.MessageServices(ctx);
                DomainServices.UserService userServices = new DomainServices.UserService(ctx);

                var user = userServices.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                user.LastViewedPaystream = System.DateTime.Now;
                userServices.UpdateUser(user);

                ctx.SaveChanges();

                return messageServices.GetMessages(user.UserId);


            }
        }
    }
}
