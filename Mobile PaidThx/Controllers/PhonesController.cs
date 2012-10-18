using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.CustomAttributes;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class PhonesController : Controller
    {
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        
        public ActionResult Index()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("Phone")).ToList();

            var model = new PhonesModels.PhonesModel()
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
            return View(new PhonesModels.AddPhoneModel()
            {
                PhoneNumber = ""
            });
        }
        [HttpPost]
        public ActionResult Add(PhonesModels.AddPhoneModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var service = new UserPayPointServices();

            try
            {
                service.AddPaypoint(user.userId.ToString(), model.PhoneNumber, "Phone");
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
            var phoneNumber = user.userPayPoints.FirstOrDefault(p => p.Id == id);

            return View(new PhonesModels.DetailsPhonesModels()
            {
                PhoneNumber = phoneNumber
            });
        }

        [HttpPost]
        public ActionResult Delete(PhonesModels.DeletePhonesModel model)
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

                return View(model);
            }

            user.userPayPoints = service.GetPayPoints(user.userId.ToString());

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult ResendVerificationCode(PhonesModels.ResendVerificationCodeModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var phoneNumber = user.userPayPoints.FirstOrDefault(p => p.Id == model.PayPointId);

            var userPayPointServices = new UserPayPointServices();
            try
            {
                userPayPointServices.ResendPhoneVerificationCode(user.userId.ToString(), phoneNumber.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View("Details", new PhonesModels.DetailsPhonesModels()
                    {
                        PhoneNumber = phoneNumber
                    });
            }

            TempData["Message"] = String.Format("We just sent a text message to {0} with an activation code needed to verify your ownership of this pay point. Once you receive the text message, click the Verify Pay Point button and complete verification.", phoneNumber.Uri);

            return View("Details", new PhonesModels.DetailsPhonesModels()
            {
                PhoneNumber = phoneNumber
            });
        }
        public ActionResult VerifyPhone(string PayPointId)
        {
            return View(new PhonesModels.VerifyPhoneModel()
            {
                PayPointId = PayPointId,
                VerificationCode = ""
            });
        }
        [HttpPost]
        public ActionResult VerifyPhone(PhonesModels.VerifyPhoneModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var phoneNumber = user.userPayPoints.FirstOrDefault(p => p.Id == model.PayPointId);

            var userPayPointServices = new UserPayPointServices();
            bool verified = false;

            try
            {
                verified = userPayPointServices.VerifyMobilePayPoint(user.userId.ToString(), model.PayPointId, model.VerificationCode);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            
            }
            if (verified)
            {
                TempData["Message"] = String.Format("You've successfully verified {0} and can begin to send and receive money at this mobile #", phoneNumber.Uri);

                user.userPayPoints = userPayPointServices.GetPayPoints(user.userId.ToString());
                phoneNumber = user.userPayPoints.FirstOrDefault(p => p.Id == model.PayPointId);

                return View("Details", new PhonesModels.DetailsPhonesModels()
                {
                    PhoneNumber = phoneNumber
                });
            }
            else
            {
                ModelState.AddModelError("", "The verification code you entered does not match our records.  Try again");

                return View(model);
            }

        }
     
    }
}
