using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Controllers
{
    public class DoGoodController : Controller
    {
        //
        // GET: /DoGood/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /DoGood/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /DoGood/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DoGood/Create

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
        // GET: /DoGood/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /DoGood/Edit/5

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
        // GET: /DoGood/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DoGood/Delete/5

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
