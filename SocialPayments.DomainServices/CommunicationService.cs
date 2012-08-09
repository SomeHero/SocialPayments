using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.DomainServices
{
    public class CommunicationService
    {
        private IDbContext _ctx;

        public CommunicationService() { }

        public CommunicationService(IDbContext context)
        {
            _ctx = context;
        }

        public List<Communication> GetCommunicationTemplates()
        {
            var communications = _ctx.Communications.Select(c => c)
                .ToList();

            return communications;
        }
        public Communication GetCommunicationTemplate(string key)
        {
            return _ctx.Communications.FirstOrDefault(c => c.Key == key);
        }
    }
}
