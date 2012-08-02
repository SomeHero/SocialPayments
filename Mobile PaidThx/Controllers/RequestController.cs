using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.Controllers
{
    public class RequestController : Controller
    {
        //
        // GET: /Request/

        public ActionResult Index()
        {
            Mobile_PaidThx.Models.RequestMoneyModel model = new Models.RequestMoneyModel
            {
                Amount = 0,
                RecipientUri = null,
                Comments = null
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(String index)
        {
            Mobile_PaidThx.Models.RequestMoneyModel model = new Models.RequestMoneyModel();
            if (index != null && index.Length > 0)
            {
                model.Amount = Double.Parse(index);
            }
            else
            {
                model.Amount = 0;
            }
            model.Comments = null;
            model.RecipientUri = null;
            return View(model);
        }

        public ActionResult AmountToRequest()
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
