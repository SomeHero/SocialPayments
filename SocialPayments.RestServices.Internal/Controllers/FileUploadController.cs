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

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class FileUploadController : ApiController
    {
        string fileName = "";
        
        public Task<HttpResponseMessage<FileUploadResponse>> PostUploadFile()
        {
            fileName = String.Format("file_{0}.jpg", DateTime.Now.ToFileTime());

            return UploadFileAsync().ContinueWith<HttpResponseMessage<FileUploadResponse>>((tsk) =>
            {
               
                HttpResponseMessage<FileUploadResponse> response = null;

                if (tsk.IsCompleted)
                {
                    response = new HttpResponseMessage<FileUploadResponse>(new FileUploadResponse() {
                       FileName = fileName 
                    }, HttpStatusCode.Created);
                }
                else if (tsk.IsFaulted || tsk.IsCanceled)
                {
                    response = new HttpResponseMessage<FileUploadResponse>(HttpStatusCode.InternalServerError);
                }

                return response;
            });
        }

        public Task UploadFileAsync()
        {
            return this.Request.Content.ReadAsStreamAsync().ContinueWith((tsk) => { SaveToFile(tsk.Result); },
                                                                         TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void SaveToFile(Stream requestStream)
        {
            
            using (FileStream targetStream = File.Create("C:\\memberImages\\" + fileName))
            {
                using (requestStream)
                {
                    requestStream.CopyTo(targetStream);
                }
            }
        }
    }
}
