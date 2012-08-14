using System.Web.Mvc;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.HtmlHelpers
{
    public static class ProfileHelperExtensions
    {
        public static MvcHtmlString GetWelcomeTabCssClass(this HtmlHelper helper, MenuModel menuModel, int tabIndex)
        {
            var cssClass = "";
            //nav-item ui-btn-active

            if (menuModel != null)
            {
                if (menuModel.SelectedTabIndex == tabIndex)
                {
                    cssClass = "nav-item ui-btn-active";
                }
                else
                {
                    cssClass = "nav-item";
                }
            }
            else
            {
                cssClass = "nav-item";
            }
            return new MvcHtmlString(cssClass);
        }

    }
}