using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Controllers
{
    public class PreferencesController : Controller
    {
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

        public ActionResult UserAgreement()
        {
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
