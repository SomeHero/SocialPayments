using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IMobileNumberSignUpKey
    {
        Guid SignUpKey { get; set; }
        string MobileNumber { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? RegistrationDate { get; set; }
        bool IsExpired { get; set; }
    }
}
