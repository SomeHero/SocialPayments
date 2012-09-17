using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class TransactionBatchModels
    {
        public class TransactionBatchModel
        {
            public List<TransactionModels.TransactionResponse> Transactions { get; set; }
        }
    }
}