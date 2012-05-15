using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class EmailService
    {
        private readonly Context _ctx;

        public EmailService(Context context)
        {
            _ctx = context;
        }
        public void SendConfirmationEmail()
        {

        }

    }
}
