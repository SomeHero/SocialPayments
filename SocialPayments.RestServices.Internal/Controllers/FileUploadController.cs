using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Net;
using NLog;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class RenamingMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public RenamingMultipartFormDataStreamProvider(string root) : base(root)
        { }
        protected override string GetLocalFileName(HttpContentHeaders headers)
        {
            return String.Format(@"image_{0}.png", System.DateTime.Now.ToFileTime());
        }
    }

    public class FileUploadController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public Task<HttpResponseMessage> PostUploadFile()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(
                    Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            var provider = new RenamingMultipartFormDataStreamProvider(@"c:\memberImages");
            
            // Read the form data and return an async task.
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(readTask =>
                {
                    if (readTask.IsFaulted || readTask.IsCanceled)
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }

                    // This illustrates how to get the file names.
                    foreach (var file in provider.BodyPartFileNames)
                    {
                        _logger.Log(LogLevel.Info, "Client file name: " + file.Key);
                        _logger.Log(LogLevel.Info, "Server file path: " + file.Value);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Created);
                });

            return task;
        }

    }
}