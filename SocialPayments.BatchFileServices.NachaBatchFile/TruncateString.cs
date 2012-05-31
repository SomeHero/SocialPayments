using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.BatchFileServices.NachaBatchFile
{
    public static class TruncateString
    {
        public static string Truncate(this string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }
    }
}
