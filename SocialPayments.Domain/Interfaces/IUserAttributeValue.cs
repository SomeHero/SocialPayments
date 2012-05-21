using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IUserAttributeValue
    {
        Guid id { get; set; }
        Guid UserId { get; set; }
        IUser User { get; set; }
        Guid UserAttributeId { get; set; }
        IUserAttribute UserAttribute { get; set; }
        string AttributeValue { get; set; }
    }
}
