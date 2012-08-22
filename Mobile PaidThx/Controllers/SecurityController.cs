using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.Controllers
{
    public class SecurityController : Controller
    {
        //
        // GET: /Security/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(SecurityModels.ChangePasswordModel model)
        {
            return RedirectToAction("Index");
        }
        public ActionResult ChangeSecurityPin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeSecurityPin(SecurityModels.ChangeSecurityPinModel model)
        {
            return RedirectToAction("Index");
        }
        public ActionResult ConfirmSecurityPin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmSecurityPin(SecurityModels.ConfirmSecurityPinModel model)
        {
            return RedirectToAction("Index");
        }
        public ActionResult ForgotSecurityPin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotSecurityPin(SecurityModels.ForgotSecurityPinModel model)
        {
            return RedirectToAction("Index"); ;
        }
    }
}
