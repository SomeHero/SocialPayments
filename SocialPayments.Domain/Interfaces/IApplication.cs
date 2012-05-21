using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IApplication
    {
        string ApplicationName { get; set; }
        string Url { get; set; }
        Guid ApiKey { get; set; }
        bool IsActive { get; set; }
    }
}
