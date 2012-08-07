using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Controllers
{
    public class RequestController : Controller
    {
        Mobile_PaidThx.Models.RequestMoneyModel savedData = new Models.RequestMoneyModel();
        //
        // GET: /Request/

        public ActionResult Index()
        {
            return View(savedData);
        }

        [HttpPost]
        public ActionResult Index(String index)
        {
            if (index != null && index.Length > 0)
            {
                savedData.Amount = Double.Parse(index);
            }
            else
            {
                savedData.Amount = 0;
            }

            return View(savedData);
        }

        [HttpPut]
        public ActionResult Index(Mobile_PaidThx.Models.RequestMoneyModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult RequestData(Mobile_PaidThx.Models.RequestMoneyModel model)
        {
            return Json(model);
        }

        public ActionResult AmountToRequest()
        {
            return View();
        }

        public ActionResult AddContactRequest()
        {
            return View();
        }

        //
        // GET: /Request/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Request/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Request/Create

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
        // GET: /Request/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Request/Edit/5

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
        // GET: /Request/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Request/Delete/5

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
