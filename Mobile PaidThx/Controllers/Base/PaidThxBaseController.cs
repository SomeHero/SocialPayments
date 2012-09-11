using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;

namespace Mobile_PaidThx.Controllers.Base
{
    public class PaidThxBaseController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        protected override void OnException(ExceptionContext filterContext)
        {
            logger.Log(LogLevel.Error, "Unhandled Exception", filterContext.Exception.Message);
        }

    }
}
