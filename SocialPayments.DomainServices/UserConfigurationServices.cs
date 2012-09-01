using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Collections.ObjectModel;

namespace SocialPayments.DomainServices
{
    public class UserConfigurationServices
    {
        public Collection<Domain.UserConfiguration> GetUserConfigurationItems(string userId)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                return user.UserConfigurations;
            }
        }
        public Domain.UserConfiguration GetUserConfigurationItem(string userId, string configItem)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var result = user.UserConfigurations
                    .FirstOrDefault(u => u.ConfigurationKey == configItem);

                if (configItem == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Configuration {0} Not Found", configItem));

                return result;
            }
        }
        public void UpdateConfigurationItem(string userId, string key, string value)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var itemToUpdate = user.UserConfigurations
                    .FirstOrDefault(u => u.ConfigurationKey ==key);

                if (itemToUpdate == null)
                {
                    itemToUpdate = new Domain.UserConfiguration()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.UserId,
                        ConfigurationKey = key,
                        ConfigurationValue = value
                    };
                    user.UserConfigurations.Add(itemToUpdate);
                }
                else
                {
                    itemToUpdate.ConfigurationValue = value;
                }

                ctx.SaveChanges();
            }
        }
    }
}
