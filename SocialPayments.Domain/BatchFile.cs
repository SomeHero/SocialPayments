using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class BatchFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime FileDateTime { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public int BatchFileStatusValue { get; set; }
        public BatchFileStatus BatchFileStatus
        {
            get { return (BatchFileStatus)BatchFileStatusValue; }
            set { BatchFileStatusValue = (int)value; }
        }
    }
}
