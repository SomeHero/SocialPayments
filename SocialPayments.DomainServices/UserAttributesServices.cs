using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class UserAttributesServices
    {
        public void UpdateUserAttribute(string userId, string userAttributeId,string attributeValue)
        {
            using (var ctx = new Context())
            {
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                Guid userAttributeGuid;

                Guid.TryParse(userAttributeId, out userAttributeGuid);

                if (userAttributeGuid == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid User Attribute {0} Specified", userAttributeId));

                var userAttribute = user.UserAttributes.FirstOrDefault(a => a.UserAttributeId.Equals(userAttributeId));

                if (userAttribute == null)
                {
                    user.UserAttributes.Add(new Domain.UserAttributeValue()
                    {
                        id = Guid.NewGuid(),
                        AttributeValue = attributeValue,
                        UserAttributeId = userAttributeGuid
                    });
                }
                else
                {
                    userAttribute.AttributeValue = attributeValue;
                }

                ctx.SaveChanges();

            }
        }
    }
}
