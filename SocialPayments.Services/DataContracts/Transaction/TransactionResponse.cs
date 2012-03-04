using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using SocialPayments.Services.DataContracts.PaymentAccount;

namespace SocialPayments.Services.DataContracts.Transaction
{
     [DataContract]
   public class TransactionResponse
    {
         [DataMember(Name = "transactionId")]
         public Guid TransactionId { get; set; }

         [DataMember(Name="paymentId")]
         public Guid PaymentId { get; set; }

         [DataMember(Name="fromAccount")]
         public PaymentAccountReponse FromAccount { get; set; }

         [DataMember(Name="amount")]
         public double Amount { get; set; }

         [DataMember(Name="achTransactionId")]
         public string ACHTransactionId { get; set; }
        
         [DataMember(Name="transactionStatus")]
         public string TransactionStatus { get; set; }

         [DataMember(Name="transactionCategory")]
         public string TransactionCategory { get; set; }

         [DataMember(Name="standardEntryClass")]
         public string StandardEntryClass { get; set; }

         [DataMember(Name="paymentChannel")]
         public string PaymentChannel { get; set; }

         [DataMember(Name="transationBatchId")]
         public string TransactionBatchId { get; set; }

         [DataMember(Name="transactionSentDate")]
         public DateTime? TransationSentDate { get; set; }

         [DataMember(Name="createDate")]
         public DateTime CreateDate { get; set; }

         [DataMember(Name="lastUpdatedDate")]
         public DateTime? LastUpdatedDate { get; set; }
    }
}