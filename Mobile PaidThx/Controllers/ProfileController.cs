using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Controllers.Base;
using System.Configuration;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using System.Web.Routing;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Controllers
{
    public class ProfileController : PaidThxBaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying Profile View"));

            return View();
        }
        [HttpPost]
        public ActionResult Index(UpdateProfileModel model)
        {
            logger.Log(LogLevel.Info, String.Format("Updating Profile"));

            return View();
            //using (var ctx = new Context())
            //{
            //    var userId = (Guid)Session["UserId"];

            //    if (Session["UserId"] == null)
            //        return RedirectToAction("SignIn", "Account", null);

            //    var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

            //    if (Session["User"] == null)
            //        return RedirectToAction("SignIn", "Account", null);


            //    if (user == null)
            //    {
            //        logger.Log(LogLevel.Error, String.Format("Invalid User."));

            //        return RedirectToAction("SignIn", "Account", null);
            //    }

            //    var editUser = ctx.Users.FirstOrDefault(u => u.UserId == userId);

            //    if (editUser == null)
            //    {
            //        logger.Log(LogLevel.Error, String.Format("Invalid User."));

            //        return RedirectToAction("SignIn", "Account", null);
            //    }

            //    user.FirstName = model.FirstName;
            //    user.LastName = model.LastName;
            //    user.SenderName = model.SenderName;
            //    user.Address = model.Address;
            //    user.City = model.City;
            //    user.State = model.State;
            //    user.Zip = model.Zip;

            //    ctx.SaveChanges();

            //    Session["User"] = editUser;

            //    return RedirectToAction("Index");
            //}
        }
    }
}
