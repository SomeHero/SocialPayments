using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace SocialPayments.Services
{
    public static class APIKeyRepository
    {
        public static bool IsValidAPIKey(string key)
        {
            // TODO: Implement IsValidAPI Key using your repository
            DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();

            Guid apiKey;

            // Convert the string into a Guid and validate it
            if (Guid.TryParse(key, out apiKey) && applicationService.GetApplication(apiKey) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static List<Guid> APIKeys
        {
            get
            {
                // Get from the cache
                // Could also use AppFabric cache for scalability
                var keys = HttpContext.Current.Cache[APIKEYLIST] as List<Guid>;

                if (keys == null)
                    keys = PopulateAPIKeys();

                return keys;
            }
        }

        private static List<Guid> PopulateAPIKeys()
        {
            List<Guid> keyList;

            DataContractSerializer dcs = new DataContractSerializer(typeof(List<Guid>));
            var server = HttpContext.Current.Server;
            using (FileStream fs = new FileStream(server.MapPath("~/App_Data/APIKeys.xml"), FileMode.Open))
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
            {
                keyList = (List<Guid>)dcs.ReadObject(reader);
            }

            // Save it in the cache
            // Could be saved in AppFabric Cache for scalability across a farm
            HttpContext.Current.Cache[APIKEYLIST] = keyList;

            return keyList;
        }

        const string APIKEYLIST = "APIKeyList";

    }
}