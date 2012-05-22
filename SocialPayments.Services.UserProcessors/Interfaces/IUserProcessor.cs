using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using SocialPayments.Domain;

namespace SocialPayments.Services.UserProcessors.Interfaces
{
    [InheritedExport(typeof(IUserProcessor))]
    public interface IUserProcessor
    {
        bool Process(User user);
    }
}
