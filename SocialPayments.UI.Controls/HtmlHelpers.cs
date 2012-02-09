using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SocialPayments.UI.Controls
{
    public static class HtmlHelpers
    {
        public static string Pager(this HtmlHelper helper, int currentPage, int currentPageSize, int totalRecords, string urlPrefix)
        {
            StringBuilder sb1 = new StringBuilder();

            int seed = currentPage % currentPageSize == 0 ? currentPage : currentPage - (currentPage % currentPageSize);

            if (currentPage > 0)
                sb1.AppendLine(String.Format("<a href=\"{0}/{1}\">Previous</a>", urlPrefix, currentPage));

            if (currentPage - currentPageSize >= 0)
                sb1.AppendLine(String.Format("<a href=\"{0}/{1}\">...</a>", urlPrefix, (currentPage - currentPageSize) + 1));

            for (int i = seed; i < Math.Round((totalRecords / 10) + 0.5) && i < seed + currentPageSize; i++)
            {
                sb1.AppendLine(String.Format("<a href=\"{0}/{1}\">{1}</a>", urlPrefix, i + 1));
            }

            if (currentPage + currentPageSize <= (Math.Round((totalRecords / 10) + 0.5) - 1))
                sb1.AppendLine(String.Format("<a href=\"{0}/{1}\">...</a>", urlPrefix, (currentPage + currentPageSize) + 1));

            if (currentPage < (Math.Round((totalRecords / 10) + 0.5) - 1))
                sb1.AppendLine(String.Format("<a href=\"{0}/{1}\">Next</a>", urlPrefix, currentPage + 2));

            return sb1.ToString();
        }
    }
}