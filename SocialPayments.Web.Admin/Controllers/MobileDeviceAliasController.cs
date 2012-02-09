using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using SocialPayments.Web.Admin.Models;

namespace SocialPayments.Web.Admin.Controllers
{

    public class MobileDeviceAliasController : Controller
    {
        //
        // GET: /MobileDeviceAlias/
        private readonly Context _ctx = new Context();
        
        public ActionResult Index(int pageIndex, int pageSize)
        {
            var model = _ctx.MobileDeviceAliases
                .OrderByDescending(m => m.MobileNumber)
                .Select(m => new MobileDeviceAliasModel()
                                 {
                                     Id = m.Id,
                                     MobileNumber = m.MobileNumber,
                                     MobileNumberAlias = m.MobileNumberAlias
                                 });

            return View(model.ToPagedList(pageIndex, pageSize));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(MobileDeviceAliasModel mobiledevicealiasmodel)
        {
            if (ModelState.IsValid)
            {
                mobiledevicealiasmodel.Id = Guid.NewGuid();
                _ctx.MobileDeviceAliases.Add(new MobileDeviceAlias()
                                                 {
                                                     Id = Guid.NewGuid(),
                                                     MobileNumber = mobiledevicealiasmodel.MobileNumber,
                                                     MobileNumberAlias = mobiledevicealiasmodel.MobileNumberAlias
                                                 });
                _ctx.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(mobiledevicealiasmodel);
        }

    }
}
