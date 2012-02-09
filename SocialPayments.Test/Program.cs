using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using SocialPayments.DomainServices;
using SocialPayments.ThirdPartyServices.FedACHService;

namespace SocialPayments.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            FedACHService fedACHService = new FedACHService();

            FedACHList fedACHList;

            var isValid = fedACHService.getACHByRoutingNumber("0112234000219", out fedACHList);

            if (!isValid)
                throw new Exception("Routing Number is Invalid");
        }
    }
}
