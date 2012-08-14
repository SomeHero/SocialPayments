using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;

namespace Mobile_PaidThx.Controllers
{
    public class PreferencesController : Controller
    {
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /Preferences/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BankAccounts()
        {
            return View();
        }

        public ActionResult Help()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ReturnData(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            return Json(model);
        }

        [HttpPost]
        public ActionResult AddPaypoint(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            UserServices service = new UserServices();
            service.AddPaypoint(_apiKey, Guid.NewGuid().ToString(), user.userId.ToString(), model.Uri, model.Type, false, "", System.DateTime.Now.ToString());

            var newDataUser = service.GetUser(user.userId.ToString());
            Session["User"] = newDataUser;
            return Json(model);
        }

        [HttpPost]
        public ActionResult RemovePaypoint(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            UserServices service = new UserServices();
            service.DeletePaypoint(_apiKey, user.userId.ToString(), model.Id);

            var newDataUser = service.GetUser(user.userId.ToString());
            Session["User"] = newDataUser;

            return Json(model);
        }

        [HttpPut]
        public ActionResult PaypointInfo(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            return View(model);
        }

        [HttpPut]
        public ActionResult AddPaypoint(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            return View(model);
        }

        public ActionResult Emails()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            List<Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse> listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("EmailAddress")).ToList();
            return View(listOfPaypoints);
        }

        public ActionResult Phones()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            List<Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse> listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("Phone")).ToList();
            return View(listOfPaypoints);
        }

        public ActionResult SocialNetworks()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            List<Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse> listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("SocialNetwork")).ToList();
            return View(listOfPaypoints);
        }

        public ActionResult MeCodes()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            List<Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse> listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("MeCode")).ToList();
            return View(listOfPaypoints);
        }

        public ActionResult UserAgreement()
        {
            return View();
        }

        public ActionResult SignOut()
        {
            Session["UserId"] = null;
            return View();
        }

        //
        // GET: /Preferences/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Preferences/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Preferences/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Preferences/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Preferences/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Preferences/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Preferences/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
