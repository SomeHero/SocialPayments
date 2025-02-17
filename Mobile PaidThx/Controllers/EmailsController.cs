﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.CustomAttributes;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class EmailsController : Controller
    {
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        
        public ActionResult Index()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("EmailAddress")).ToList();

            var model = new EmailsModels.EmailsModel()
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
            return View(new EmailsModels.AddEmailAddressModel() {
                EmailAddress = ""
            });
        }
        [HttpPost]
        public ActionResult Add(EmailsModels.AddEmailAddressModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var service = new UserPayPointServices();

            try
            {
                service.AddPaypoint(user.userId.ToString(), model.EmailAddress, "EmailAddress");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            user.userPayPoints = service.GetPayPoints(user.userId.ToString());

            TempData["Success"] = "added";

            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var meCode = user.userPayPoints.FirstOrDefault(p => p.Id == id);

            return View(new EmailsModels.DetailsEmailModel()
            {
                EmailAddress = meCode
            });
        }

        [HttpPost]
        public ActionResult Delete(EmailsModels.DeleteEmailModel model)
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
        public ActionResult ResendVerification(EmailsModels.ResendEmailVerificationModel model)
        {
            var user = (UserModels.UserResponse)Session["User"];
            var meCode = user.userPayPoints.FirstOrDefault(p => p.Id == model.PayPointId);

            var service = new UserPayPointServices();

            try
            {
                service.ResendEmailVerificationLink(user.userId.ToString(), meCode.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View("Details",
                    new EmailsModels.DetailsEmailModel()
                    {
                        EmailAddress = meCode
                    });
            }

            TempData["Success"] = String.Format("A verification email was sent to this email. Please check your email and click on the link.");

            return View("Details",
                    new EmailsModels.DetailsEmailModel()
                    {
                        EmailAddress = meCode
                    });
        }

    }
}
