using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using Quartz;
using System.IO;
using Amazon.S3.Model;
using System.Configuration;
using NLog;

namespace SocialPayments.BatchFileServices.NachaBatchFile
{
    public class CreateNachaFileJob: IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Execute(JobExecutionContext context)
        {
            //Get Current Batch
            //Close Current Batch
            //And Open New One
            //Create a new BatchFile Record
            logger.Log(LogLevel.Info, String.Format("Creating Nacha ACH File at {0}", System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")));
            
            var transactionBatchService = new TransactionBatchService();
            logger.Log(LogLevel.Info, String.Format("Batching Transactions"));
            
            var transactions = transactionBatchService.BatchTransactions();

            FileGenerator fileGeneratorService = new FileGenerator();
            fileGeneratorService.CompanyIdentificationNumber = ConfigurationManager.AppSettings["CompanyIdentificationNumber"];
            fileGeneratorService.CompanyName = ConfigurationManager.AppSettings["CompanyName"];
            fileGeneratorService.ImmediateDestinationId = ConfigurationManager.AppSettings["ImmediateDestinationId"];
            fileGeneratorService.ImmediateDestinationName = ConfigurationManager.AppSettings["ImmediateDestinationName"];
            fileGeneratorService.ImmediateOriginId = ConfigurationManager.AppSettings["ImmediateOriginId"];
            fileGeneratorService.ImmediateOriginName = ConfigurationManager.AppSettings["ImmediateOriginName"];
            fileGeneratorService.OriginatingTransitRoutingNumber = ConfigurationManager.AppSettings["OriginatingTransitRoutingNumber"];
            fileGeneratorService.CompanyDiscretionaryData = ConfigurationManager.AppSettings["CompanyDiscretionaryData"];

            logger.Log(LogLevel.Info, String.Format("Processing Transactions"));
            
            var results = fileGeneratorService.ProcessFile(transactions);

            logger.Log(LogLevel.Info, String.Format("Creating Batch File"));
            
            StringBuilder sb = new StringBuilder();
            results.ForEach(s => sb.AppendLine(s));
            string fileContext = sb.ToString();

            logger.Log(LogLevel.Info, String.Format("Uploading Batch File to S3 Server"));
            
            string bucketName = ConfigurationManager.AppSettings["NachaFileBucketName"];
            if (String.IsNullOrEmpty(bucketName))
                throw new Exception("S3 bucket name for NachaFileBucketName not configured");

            Amazon.S3.AmazonS3Client s3Client = new Amazon.S3.AmazonS3Client();
            PutObjectRequest putRequest = new PutObjectRequest()
            {
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                BucketName = bucketName,
                ContentBody = fileContext,
                Key = "NACHA_FILE_" + System.DateTime.Now.ToString("MMddyy_Hmmss")
            };
            s3Client.PutObject(putRequest); 

            //Move all payments to paid
            //Update Batch File
            //Start BatchFile Workflow
        }
    }
}
