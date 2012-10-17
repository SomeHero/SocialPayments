using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.CustomAttributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (HttpContext.Current.Session["UserId"] == null)
                return false;
            if (HttpContext.Current.Session["User"] == null)
                return false;
            if (HttpContext.Current.Session["Application"] == null)
                return false;


            return base.AuthorizeCore(httpContext);
        }
    }
}