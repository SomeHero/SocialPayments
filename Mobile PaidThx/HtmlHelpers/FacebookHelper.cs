using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.HtmlHelpers
{
    public static class FacebookHelperExtensions
    {
        public static MvcHtmlString FormatCallbackUrl(this HtmlHelper helper, string absoluteUri, string urlFragment, string fbState)
        {
            return new MvcHtmlString(absoluteUri + urlFragment + fbState);
        }
    }
}