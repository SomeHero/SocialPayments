using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace SocialPayments.BatchFileServices.NachaBatchFile.Services.ResponseModels
{
    public class ServiceResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Description { get; set; }
        public string JsonResponse { get; set; }
    }
}