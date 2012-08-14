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
            if (menuModel.SelectedTabIndex == tabIndex)
            {
                cssClass = "nav-item ui-btn-active";
            }
            else
            {
                cssClass = "nav-item";
            }
            return new MvcHtmlString(cssClass);
        }

        public static MvcHtmlString GetTransactionCssClass(this HtmlHelper helper, PaystreamModels.PaymentModel receipt, int rowindex)
        {
            var cssClass = "";
            if (receipt.TransactionType == TransactionType.Withdrawal)
                cssClass = "receive";
            else if (receipt.TransactionType == TransactionType.Deposit)
                cssClass = "paid";

            if (rowindex % 2 == 0)
            {
                cssClass += " paystream-row";
            }
            else
            {
                cssClass += " paystream-row-alt";
            }

            switch (receipt.TransactionStatus)
            {
                case TransactionStatus.Cancelled:
                    cssClass += " cancelled";
                    break;
                case TransactionStatus.Complete:
                    cssClass += " complete";
                    break;
                case TransactionStatus.Pending:
                    cssClass += " pending";
                    break;
                case TransactionStatus.Returned:
                    cssClass += " returned";
                    break;
                case TransactionStatus.Submitted:
                    cssClass += " submitted";
                    break;
            }

            return new MvcHtmlString(cssClass);
        }
    }
}