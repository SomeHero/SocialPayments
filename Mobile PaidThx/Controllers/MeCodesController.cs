using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services;

namespace Mobile_PaidThx.Controllers
{
    public class MeCodesController : Controller
    {
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        
        public ActionResult Index()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("MeCode")).ToList();

            var model = new MeCodesModels.MeCodesModel()
            {
                PayPoints = listOfPaypoints.Select(e => new UserModels.UserPayPointResponse()
                {
                    CreateDate = e.CreateDate,
                    Id = e.Id,
                    Type = e.Type,
                    Uri = e.Uri,
                    UserId = e.UserId,
                    Verified = e.Verified,
                    VerifiedDate = e.VerifiedDate
                }).ToList()
            };
            return View(model);
        }

        public ActionResult Add()
        {
            return View(new MeCodesModels.AddMeCodeModel()
            {
                MeCode = ""
            });
        }
        [HttpPost]
        public ActionResult Add(MeCodesModels.AddMeCodeModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var service = new UserPayPointServices();

            try
            {
                service.AddPaypoint(user.userId.ToString(), model.MeCode, "MeCode");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            user.userPayPoints = service.GetPayPoints(user.userId.ToString());

            return RedirectToAction("Index");
        }

        public ActionResult Details(string id)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var meCode = user.userPayPoints.FirstOrDefault(p => p.Id == id);

            return View(new MeCodesModels.DetailsMeCodeModel()
            {
                MeCode = meCode
            });
        }

        [HttpDelete]
        public ActionResult Delete(MeCodesModels.DeleteMeCodeModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var service = new UserPayPointServices();

            try
            {
                service.DeletePaypoint(_apiKey, user.userId.ToString(), model.PayPointId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var meCode = user.userPayPoints.FirstOrDefault(p => p.Id == model.PayPointId);

                return View("Details", new MeCodesModels.DetailsMeCodeModel()
                {
                    MeCode = meCode
                });
            }

            user.userPayPoints = service.GetPayPoints(user.userId.ToString());

            return RedirectToAction("Index");
        }
    }
}
