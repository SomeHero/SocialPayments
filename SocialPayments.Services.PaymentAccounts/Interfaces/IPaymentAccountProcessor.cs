using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using System.ComponentModel.Composition;

namespace SocialPayments.Services.PaymentAccounts.Interfaces
{
    [InheritedExport(typeof(IPaymentAccountProcessor))]
    public interface IPaymentAccountProcessor
    {
        bool Process(PaymentAccount paymentAccount);
    }
}
