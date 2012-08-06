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
            Mobile_PaidThx.Models.SendMoneyModel model = new Models.SendMoneyModel
            {
                Amount = 0,
                Comments = null,
                RecipientUri = null
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(String index)
        {
            Mobile_PaidThx.Models.SendMoneyModel model = new Models.SendMoneyModel();
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

        //
        // GET: /Send/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult AddContactSend()
        {
            return View();
        }

        [HttpPut]
        public ActionResult Index(Mobile_PaidThx.Models.SendMoneyModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult SendData(Mobile_PaidThx.Models.SendMoneyModel model)
        {
            return Json(model);
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
        public ActionResult AmountToSend()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(String id)
        {
            return View("Index");
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
