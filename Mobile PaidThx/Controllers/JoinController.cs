using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.Controllers
{
    public class JoinController : Controller
    {
        //
        // GET: /Join/
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string _userServiceUrl = "http://23.21.203.171/api/internal/api/Users";
        private string _setupACHAccountServiceUrl = "http://23.21.203.171/api/internal/api/Users/{0}/PaymentAccounts";

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(RegisterModel model)
        {
            _logger.Log(LogLevel.Info, String.Format("Register User {0}", model.Email));

            string userId = String.Empty;

            try
            {
                var userServices = new Services.UserServices();
                userId = userServices.RegisterUser(_userServiceUrl, _apiKey, model.Email, model.Password, model.Email, "MobileWeb", "");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Registering User {0}. {1}", model.Email, ex.Message));

                ModelState.AddModelError("", "Error Registering User");

                return View("Index");
            }

            Session["UserId"] = userId;

            return RedirectToAction("Personalize", "Register");
        }
        //
        // GET: /Join/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Join/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Join/Create

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
        // GET: /Join/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Join/Edit/5

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
        // GET: /Join/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Join/Delete/5

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
