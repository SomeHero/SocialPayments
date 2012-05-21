using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IUserAttribute
    {
        Guid Id { get; set; }
        string AttributeName { get; set; }
        bool Approved { get; set; }
        bool IsActive { get; set; }
    }
}
