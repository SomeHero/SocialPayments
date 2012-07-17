using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum MessageType
    {
        Payment = 0,
        PaymentRequest = 1,
        Update = 2,
        //Donation =3,
        //AcceptPledge = 4
    }
}
