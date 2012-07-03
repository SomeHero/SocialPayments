using System.Web.Mvc;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.HtmlHelpers
{
    public static class ProfileHelperExtensions
    {

        public static MvcHtmlString GetTransactionCssClass(this HtmlHelper helper, PaystreamModels.PaymentModel receipt)
        {
            var cssClass = "";
            if (receipt.TransactionType == TransactionType.Withdrawal)
                cssClass = "receive";
            else if (receipt.TransactionType == TransactionType.Deposit)
                cssClass = "paid";

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