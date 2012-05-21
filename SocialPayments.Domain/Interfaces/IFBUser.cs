using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IFBUser
    {
        Guid Id { get; set; }
        string FBUserID { get; set; }
        string OAuthToken { get; set; }
        DateTime TokenExpiration { get; set; }
        IUser User { get; set; }
    }
}
