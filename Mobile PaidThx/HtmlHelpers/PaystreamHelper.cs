using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.HtmlHelpers
{
    public static class PaystreamHelperExtensions
    {
        public static MvcHtmlString FormatUri(this HtmlHelper helper, string uri)
        {

            SocialPayments.DomainServices.FormattingServices formattingService = new SocialPayments.DomainServices.FormattingServices();

            var tempUri = formattingService.FormatMobileNumber(uri);

            if (tempUri.Length >= 10)
                return new MvcHtmlString(String.Format("({0}) {1}-{2}", tempUri.Substring(0, 3), tempUri.Substring(3, 3), tempUri.Substring(6, 4)));
            
            return new MvcHtmlString(uri);
        }
    }
}