using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Transaction
{
    [DataContract]
    public class TransactionRequest
    {
        [DataMember(Name="userId")]
        public String UserId { get; set; }
    }
}