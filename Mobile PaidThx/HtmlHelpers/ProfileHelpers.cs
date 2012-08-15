using System.Web.Mvc;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.HtmlHelpers
{
    public static class ProfileHelperExtensions
    {
        public static MvcHtmlString GetTableClass(this HtmlHelper helper, int currentIndex, int totalSize)
        {
            if (totalSize == 1)
            {
                return new MvcHtmlString("tablesettings");
            }
            else
            {
                if (currentIndex == 0)
                {
                    return new MvcHtmlString("tabletop");
                }
                else
                {
                    if (currentIndex != totalSize - 1)
                    {
                        return new MvcHtmlString("tablemiddle");
                    }
                    else
                    {
                        return new MvcHtmlString("tablebottom");
                    }
                }
            }
        }

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