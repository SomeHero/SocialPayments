using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class UserSocialNetworkServices
    {
        public void AddUserSocialNetwork(string userId, string socialNetworkName, string socialNetworkUserId, string socialNetworkToken)
        {
            using (var ctx = new Context())
            {
                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userGuid);

                if(user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var socialNetwork = ctx.SocialNetworks.FirstOrDefault(s => s.Name == socialNetworkName);

                if (socialNetwork == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Social Network {0} Not Valid", socialNetworkName));

                ctx.UserSocialNetworks.Add(new Domain.UserSocialNetwork()
                {
                    EnableSharing = false,
                    SocialNetwork = socialNetwork,
                    User = user,
                    UserNetworkId = socialNetworkUserId,
                    UserAccessToken = socialNetworkToken
                });

                ctx.SaveChanges();
            }
        }
    }
}
