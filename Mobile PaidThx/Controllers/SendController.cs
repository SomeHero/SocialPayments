using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Controllers
{
    public class SendController : Controller
    {
        //
        // GET: /Send/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Send/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Send/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Send/Create

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
        // GET: /Send/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Send/Edit/5

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
        // GET: /Send/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Send/Delete/5

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
