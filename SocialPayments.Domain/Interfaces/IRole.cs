using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain.Interfaces
{
    public interface IRole
    {
        Guid RoleId { get; set; }
        string RoleName { get; set; }
        Collection<IUser> Users { get; set; }
        string Description { get; set; }
    }
}
