using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using NLog;

namespace SocialPayments.BatchFileServices.NachaBatchFile
{
    public class FileGenerator
    {
        public char PaddingCharacter { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDiscretionaryData { get; set; }
        public string CompanyIdentificationNumber { get; set; }
        public string OriginatingTransitRoutingNumber { get; set; }
        public string ImmediateDestinationId { get; set; }
        public string ImmediateDestinationName { get; set; }
        public string ImmediateOriginId { get; set; }
        public string ImmediateOriginName { get; set; }

        private int achTxIDCount;
        private int numberOfDetailRecords;
        private int numberInCurrentBatch;
        private int numberOfBatches;
        private int numberOfRecords;
        private SecurityService securityService = new SecurityService();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public FileGenerator()
        {
            PaddingCharacter = ' ';
            CompanyName = "PdThxME";
            CompanyDiscretionaryData = "Test File";
            CompanyIdentificationNumber = "11111111";
            ImmediateDestinationId = "bTTTTAAAAC";
            ImmediateOriginId = "NTTTTAAAAC";
            ImmediateDestinationName = "DESTINATIONNAME";
            ImmediateOriginName = "OriginName";
            OriginatingTransitRoutingNumber = "053000111";

        }
        public List<string> ProcessFile(ICollection<Transaction> transactions)
        {
            DateTime processedDate = System.DateTime.Now;
            List<string> results = new List<string>();
            var batchNumber = 1;

            _logger.Log(LogLevel.Info, String.Format("Start Process NACHA File for {0} with {1} transactions", processedDate.ToString("MM/dd/yy hh:mm"), transactions.Count));

            results.Add(CreateFileHeaderRecord(processedDate));
            results.Add(CreateBatchHeaderRecord(transactions, "PPD", "Test Transactions", processedDate, processedDate.AddDays(2), OriginatingTransitRoutingNumber, batchNumber));
            foreach(var transaction in transactions) {
                results.Add(CreateBatchDetailRecord(transaction));
            }
            results.Add(CreateBatchTrailerRecord(transactions, batchNumber));
            results.Add(CreateFileTrailerRecord(processedDate, transactions));


            var blockCountRecords = 10 - numberOfRecords%10;

            for (var i = 1; i <= blockCountRecords; i++)
            {
                results.Add(CreateBlockTrailerRecord());
            }

            _logger.Log(LogLevel.Info, String.Format("Complete Process NACHA File for {0} with {1} transactions", processedDate.ToString("MM/dd/yy hh:mm"), transactions.Count));
            
            return results;
        }

        private string CreateFileHeaderRecord(DateTime processedDate)
        {
            _logger.Log(LogLevel.Info, String.Format("Start File Header Record for {0}", processedDate.ToString("MM/dd/yy hh:mm")));

            numberOfRecords += 1;

            StringBuilder sb = new StringBuilder();

            sb.Append("1".PadLeft(1, ' '));   //Record Type Code 1-1
            sb.Append("01".PadLeft(2, ' '));  //Priority Code 2-3
            sb.Append(ImmediateDestinationId.Truncate(10).PadLeft(10, ' ')); //Immediate Destination ID
            sb.Append(ImmediateOriginId.Truncate(10).PadLeft(10, ' ')); //Immediate Origin ID
            sb.Append(processedDate.ToString("yyMMdd").PadLeft(4, ' '));
            sb.Append(processedDate.ToString("hhmm").PadLeft(4, ' '));
            sb.Append("A".PadLeft(1, ' '));   //file id modifer
            sb.Append("094".PadLeft(3, ' ')); //record size
            sb.Append("10".PadLeft(2, ' '));  //blocking factor
            sb.Append("1".PadLeft(1, ' '));
            sb.Append(ImmediateDestinationName.Truncate(23).PadRight(23, ' '));  //Immediate Destination
            sb.Append(ImmediateOriginName.Truncate(23).PadRight(23, ' '));   //Immediate Origination
            sb.Append("".PadLeft(8, ' '));

            _logger.Log(LogLevel.Info, String.Format("Complete File Header Record for {0}", processedDate.ToString("MM/dd/yy hh:mm")));

            return sb.ToString();
        }
        private string CreateBatchHeaderRecord(ICollection<Transaction> transactions, string standardEntryClass, string entryDescription, DateTime companyDescriptiveDate,
            DateTime effectiveEntryDate, string originatingTransitRoutingNumber, int batchNumber)
        {
            _logger.Log(LogLevel.Info, String.Format("Start Batch Header Record for {0} with {1}", batchNumber, transactions.Count));

            numberOfRecords += 1;
            numberInCurrentBatch = 0;

            StringBuilder sb = new StringBuilder();

            sb.Append("5".PadLeft(1, PaddingCharacter));   //Record Type Code
            sb.Append("200".PadLeft(3, PaddingCharacter)); //Service Class Code
            sb.Append(CompanyName.Truncate(16).PadRight(16, ' '));  //Company Name
            sb.Append(CompanyDiscretionaryData.Truncate(20).PadRight(20, ' '));    //Descrtionary Data
            sb.Append(CompanyIdentificationNumber.Truncate(10).PadLeft(10, ' '));   //Company Identification
            sb.Append(standardEntryClass.Truncate(3).PadLeft(3, ' '));   //Standard Entry Class
            //sb.Append(client.EBPPData.ACHEntryDescription.PadRight(10, ' '));         //Company Entry Description
            sb.Append(entryDescription.Truncate(10).PadRight(10, ' '));         //Company Entry Description
            sb.Append(companyDescriptiveDate.ToString("yyMMdd"));  //Company Descriptive Date*
            sb.Append(effectiveEntryDate.ToString("yyMMdd"));  //Effective Entry Date*
            sb.Append("".PadLeft(3, ' '));    //SettlementDate (Julian)
            sb.Append("1".PadLeft(1, ' '));         //Originator Status Code
            sb.Append(originatingTransitRoutingNumber.Truncate(8).PadLeft(8, ' '));   //Originating Financial Institution
            sb.Append(batchNumber.ToString().PadLeft(7, '0'));   //Batch Number

            _logger.Log(LogLevel.Info, String.Format("Complete Batch Header Record for {0} with {1}", batchNumber, transactions.Count));

            return sb.ToString();
        }
        private string CreateBatchTrailerRecord(ICollection<Transaction> transactions, int batchNumber)
        {
            _logger.Log(LogLevel.Info, String.Format("Start Batch Trailer Record for {0} with {1}", batchNumber, transactions.Count));

            numberOfRecords += 1;

            string entryHash = CalcuateEntryHash(transactions);
            double totalDebitAmount = transactions.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount);
            double totalCreditAmount = transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);

            StringBuilder sb = new StringBuilder();

            sb.Append("8".PadLeft(1, PaddingCharacter));         //Record Type Code
            sb.Append("200".PadLeft(3, PaddingCharacter));    //Service Class Code
            sb.Append(numberInCurrentBatch.ToString().PadLeft(6, '0'));  //Entry/Addenda Count
            sb.Append(entryHash.PadLeft(10, '0')); //Entry Hash
            sb.Append(totalDebitAmount.ToString("0.00").Replace(".", "").PadLeft(12, '0'));   //Total Debit Amount
            sb.Append(totalCreditAmount.ToString("0.00").Replace(".", "").PadLeft(12, '0'));     //Total Credit Amount
            sb.Append(CompanyIdentificationNumber.Truncate(10).PadLeft(10, ' '));   //Company Identification
            sb.Append("".PadLeft(19, ' '));                //Message Authentication Code
            sb.Append("".PadLeft(6, ' '));                 //Reserved


            string originationFinancialInstitution = "";
            if (ImmediateDestinationId.ToString().Length > 8)
                originationFinancialInstitution = ImmediateDestinationId.Substring(0, 8);
            sb.Append(originationFinancialInstitution.Truncate(8).PadLeft(8, '0'));          //Originating DFI Identification
            sb.Append(batchNumber.ToString().PadLeft(7, '0'));   //Batch Number

            numberOfBatches += 1;

            _logger.Log(LogLevel.Info, String.Format("Start Batch Trailer Record for {0} with {1}", batchNumber, transactions.Count));

            return sb.ToString();
        }

        private string CalcuateEntryHash(ICollection<Transaction> transactions)
        {
            Int64 entryHash = 0;
            foreach (Transaction transaction in transactions)
            {
                try
                {
                    string routingNumber = securityService.Decrypt(transaction.FromAccount.RoutingNumber);
                    if (routingNumber.Length > 8)
                        routingNumber = routingNumber.Substring(0, 8);

                    entryHash += Convert.ToInt32(routingNumber);
                }
                catch (Exception ex)
                {
                    //_logger.Log(LogLevel.Error, "Error Occurred in CreateBatchTrailerRecord: Payment ID: " + payment.ID + ": " + ex.Message);
                }
            }
            string entryHashString = entryHash.ToString();
            if (entryHashString.Length > 10)
                entryHashString = entryHash.ToString().Substring(entryHashString.Length - 10, 10);

            return entryHashString;
        }

        private string CreateFileTrailerRecord(DateTime processedDate, ICollection<Transaction> transactions)
        {
            _logger.Log(LogLevel.Info, String.Format("Start File Trailer Record for {0} with {1}", processedDate.ToString("MM/dd/yy hh:mm"), transactions.Count));

            numberOfRecords += 1;

            string entryHash = CalcuateEntryHash(transactions);
            double totalDebitAmount = transactions.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount);
            double totalCreditAmount = transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);

            StringBuilder sb = new StringBuilder();

            sb.Append("9".PadLeft(1, '0'));         //Record Type Code
            sb.Append(numberOfBatches.ToString().PadLeft(6, '0'));    //Batch Count

            //Calculate the Block Count
            int blockRecordCount = numberOfRecords;

            while ((blockRecordCount % 10) != 0)
            {
                blockRecordCount += 1;
            }
            int blockCount = blockRecordCount / 10;

            sb.Append(blockCount.ToString().PadLeft(6, '0'));    //Block Count
            sb.Append(numberOfDetailRecords.ToString().PadLeft(8, '0'));  //Entry Addenda Count
            sb.Append(entryHash.PadLeft(10, '0')); //Entry Hash
            sb.Append(totalDebitAmount.ToString("0.00").Replace(".", "").PadLeft(12, '0'));   //Total Debit Amount
            sb.Append(totalCreditAmount.ToString("0.00").Replace(".", "").PadLeft(12, '0'));     //Total Credit Amount
            sb.Append("".PadLeft(39, ' '));               //Reserved

            _logger.Log(LogLevel.Info, String.Format("Complete File Trailer Record for {0} with {1}", processedDate.ToString("MM/dd/yy hh:mm"), transactions.Count));

            return sb.ToString();
        }
        private string CreateBatchDetailRecord(Transaction transaction)
        {
            _logger.Log(LogLevel.Info, String.Format("Start Batch Detail Record for transaction {0}", transaction.Id));

            achTxIDCount += 1;
            numberOfRecords += 1;
            numberInCurrentBatch += 1;
            numberOfDetailRecords += 1;

            StringBuilder sb = new StringBuilder();

            sb.Append("6".PadLeft(1, ' '));     //Record Type Code
            if (transaction.Type == TransactionType.Deposit)
            {
                if (transaction.FromAccount.AccountType == PaymentAccountType.Checking)
                    sb.Append("22".PadLeft(2, ' ')); //Transaction Code Deposit "Checking" account
                else
                    sb.Append("32".PadLeft(2, ' ')); //Transaction Code Deposit "Savings" account
            }
            else
            {
                if (transaction.FromAccount.AccountType == PaymentAccountType.Checking)
                    sb.Append("27".PadLeft(2, ' ')); //Transaction Code Debit "Checking" account
                else
                    sb.Append("37".PadLeft(2, ' ')); //Transaction Code Debit "Savings" account
            }
        
            string routingNumber = securityService.Decrypt(transaction.FromAccount.RoutingNumber);
            sb.Append(routingNumber.PadLeft(9, ' '));  //Receiving DFI Identification

            string accountNumber = securityService.Decrypt(transaction.FromAccount.AccountNumber);
            sb.Append(accountNumber.PadRight(17, ' '));  //DFI Account Number
            sb.Append((transaction.Amount * 100).ToString().PadLeft(10, '0'));  //Amount
            sb.Append(transaction.FromAccount.User.MobileNumber.PadRight(15, ' '));  //Individual Identification --Consumer Account Number should be ACH Company name

            string nameOnAccount = securityService.Decrypt(transaction.FromAccount.NameOnAccount);
            sb.Append(nameOnAccount.Truncate(22).PadRight(22, ' '));   //Individual Name
           // if (transaction.StandardEntryClass == StandardEntryClass.Web)  //Discretionary Data - Required if Web
           // {
               // if (transaction.PaymentChannelType == PaymentChannelType.Recurring)
                 //   sb.Append("R".PadRight(2, ' '));  //R for Recurring Payment
               // else
                    //sb.Append("S".PadRight(2, ' '));  //S for Single
           // }
            //else
                sb.Append("".PadLeft(2, ' '));  //R for Recurring Payment

            sb.Append("0".PadLeft(1, ' '));  //Appends Record Indicator

            if (this.achTxIDCount > 9999999)
                this.achTxIDCount = 1;


            string traceNumber = "";
            if (ImmediateDestinationId.Length > 8)
                traceNumber = ImmediateDestinationId.Substring(0, 8) + this.achTxIDCount.ToString().PadLeft(7, '0');
            else
                traceNumber = ImmediateDestinationId + this.achTxIDCount.ToString().PadLeft(7, '0');
            sb.Append(traceNumber);   //Trace Number

            //Update Payment With ACHSubmitTransactionId and ACHFileName and ACH D/T
            transaction.ACHTransactionId = achTxIDCount.ToString();

            _logger.Log(LogLevel.Info, String.Format("Complete Batch Detail Record for transaction {0}", transaction.Id));

            return sb.ToString();
        }
        private string CreateBlockTrailerRecord()
        {
            
            numberOfRecords += 1;

            return "9999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999";
        }
    }
}
