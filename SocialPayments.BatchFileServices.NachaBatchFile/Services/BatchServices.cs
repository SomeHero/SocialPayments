﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.BatchFileServices.NachaBatchFile.Models;
using System.Net;
using System.Web.Script.Serialization;

namespace SocialPayments.BatchFileServices.NachaBatchFile.Services
{
    public class BatchServices : ServicesBase
    {
        private string _batchServicesBaseUrl = "{0}Batch/batch_transactions";
       
        public TransactionBatch BatchTransactions()
        {
            var js = new JavaScriptSerializer();
            var serviceUrl = String.Format(_batchServicesBaseUrl, _webServicesBaseUrl);

            var response = Post(serviceUrl, "");

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            return js.Deserialize<TransactionBatch>(response.JsonResponse);
        }
    }
}