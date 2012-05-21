using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IBetaSignup
    {
        Guid Id { get; set; }
        string EmailAddress { get; set; }
        DateTime CreateDate { get; set; }
    }
}
