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
using SocialPayments.DataLayer;

namespace SocialPayments.BatchFileServices.NachaBatchFile
{
    public class CreateNachaFileJob: IJob
    {
        private readonly Context _ctx = new Context();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private TransactionBatchService transactionBatchService;
        public CreateNachaFileJob()
        {
            transactionBatchService = new DomainServices.TransactionBatchService(_ctx, logger);
        }
        public void Execute(JobExecutionContext context)
        {
            //Get Current Batch
            //Close Current Batch
            //And Open New One
            //Create a new BatchFile Record
            logger.Log(LogLevel.Info, String.Format("Creating Nacha ACH File at {0}", System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")));

            logger.Log(LogLevel.Info, String.Format("Batching Transactions"));
            var transactionBatch = transactionBatchService.BatchTransactions();

            FileGenerator fileGeneratorService = new FileGenerator();
            fileGeneratorService.CompanyIdentificationNumber = ConfigurationManager.AppSettings["CompanyIdentificationNumber"];
            fileGeneratorService.CompanyName = ConfigurationManager.AppSettings["CompanyName"];
            fileGeneratorService.ImmediateDestinationId = ConfigurationManager.AppSettings["ImmediateDestinationId"];
            fileGeneratorService.ImmediateDestinationName = ConfigurationManager.AppSettings["ImmediateDestinationName"];
            fileGeneratorService.ImmediateOriginId = ConfigurationManager.AppSettings["ImmediateOriginId"];
            fileGeneratorService.ImmediateOriginName = ConfigurationManager.AppSettings["ImmediateOriginName"];
            fileGeneratorService.OriginatingTransitRoutingNumber = ConfigurationManager.AppSettings["OriginatingTransitRoutingNumber"];
            fileGeneratorService.CompanyDiscretionaryData = ConfigurationManager.AppSettings["CompanyDiscretionaryData"];

            logger.Log(LogLevel.Info, String.Format("Processing Transactions for batch {0}", transactionBatch.Id));

            List<string> results = null;

            try
            {
                results = fileGeneratorService.ProcessFile(transactionBatch.Transactions);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Exception processing Nacha File. {0}", ex.Message));

                throw ex;
            }

            logger.Log(LogLevel.Info, String.Format("Creating batch file for batch {0}", transactionBatch.Id));

            StringBuilder sb = new StringBuilder();
            results.ForEach(s => sb.AppendLine(s));
            string fileContext = sb.ToString();

            string bucketName = ConfigurationManager.AppSettings["NachaFileBucketName"];
            
            logger.Log(LogLevel.Info, String.Format("Uploading Batch File for batch {0} to bucket {1}", transactionBatch.Id, bucketName));
            
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
            try
            {
                s3Client.PutObject(putRequest);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Unable to upload nacha file to S3. {0}", ex.Message));
            }

            //Move all payments to paid
            //Update Batch File
            //Start BatchFile Workflow
        }
    }
}
