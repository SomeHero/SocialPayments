using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace SocialPayments.Web.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static MvcHtmlString DropDownList(this HtmlHelper helper,
            string name, Dictionary<int, string> dictionary)
        {
            var selectListItems = new SelectList(dictionary, "Key", "Value");
            return helper.DropDownList(name, selectListItems);
        }
    }
}