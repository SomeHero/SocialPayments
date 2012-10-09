using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class RenamingMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public RenamingMultipartFormDataStreamProvider(string root) : base(root)
        { }
        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            return String.Format(@"image_{0}.png", System.DateTime.Now.ToFileTime());
        }
    }

    public class FileUploadController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<HttpResponseMessage> PostUploadFile()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(
                    Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            var provider = new RenamingMultipartFormDataStreamProvider(@"c:\memberImages");
            var bodyParts = await Request.Content.ReadAsMultipartAsync(provider);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

    }
}