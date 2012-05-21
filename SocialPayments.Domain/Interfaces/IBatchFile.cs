using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IBatchFile
    {
        int Id { get; set; }
        string FileName { get; set; }
        string FileType { get; set; }
        DateTime FileDateTime { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? LastUpdatedDate { get; set; }
        int BatchFileStatusValue { get; set; }
        BatchFileStatus BatchFileStatus { get; set; }
    }
}
