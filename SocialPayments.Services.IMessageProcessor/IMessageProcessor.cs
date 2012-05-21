using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using SocialPayments.Domain;
namespace SocialPayments.Services.IMessageProcessor
{
    [InheritedExport(typeof(IMessageProcessor))]
    public interface IMessageProcessor
    {
        bool Process(Message message);
    }
}
