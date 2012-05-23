using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DataLayer;
using NLog;

namespace SocialPayments.DomainServices
{
    public class MessageServices
    {
        private IDbContext _context;
        private ValidationService _validationService;
        private Logger _logger;

        public MessageServices()
        {
            _context = new Context();
            _logger = LogManager.GetCurrentClassLogger();
            _validationService = new ValidationService(_logger);
        }
        public MessageServices(IDbContext context)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();
            _validationService = new ValidationService(_logger);
        }
        public URIType GetURIType(string uri)
        {
            var uriType = URIType.MobileNumber;

            if (_validationService.IsEmailAddress(uri))
                uriType = URIType.EmailAddress;
            else if (_validationService.IsMECode(uri))
                uriType = URIType.MECode;

            return uriType;
        }
    }
}
