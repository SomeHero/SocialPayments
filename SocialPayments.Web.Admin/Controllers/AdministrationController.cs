using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SocialPayments.DataLayer;
using SocialPayments.Web.Admin.Models;
using System.Web.Security;
using SocialPayments.Domain;
using System.Configuration;

namespace SocialPayments.Web.Admin.Controllers
{
    public class AdministrationController : Controller
    {
        //
        // GET: /Administration/
        private readonly Context _ctx = new Context();

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                var adminRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Administrator");

                _ctx.Users.Add(new User()
                {
                    ApiKey = new Guid(ConfigurationManager.AppSettings["PaidThxApiKey"]),
                    CreateDate = System.DateTime.Now,
                    LastLoggedIn = System.DateTime.Now,
                    EmailAddress = model.Email,
                    IsConfirmed = true,
                    IsLockedOut = false,
                    Roles = new List<Role>() {
                        adminRole
                    },
                    Password = model.Password,
                    UserId = Guid.NewGuid(),
                    UserName = model.UserName,
                    UserStatus = UserStatus.Verified
                });

                _ctx.SaveChanges();

                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(EditModel model)
        {
            if (ModelState.IsValid)
            {
                Guid userId;

                Guid.TryParse(model.Id, out userId);

                var user = _ctx.Users.FirstOrDefault(u => u.UserId == userId);
                user.EmailAddress = model.Email;

                _ctx.SaveChanges();

                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        public ActionResult Index(int pageIndex, int pageSize)
        {
            var model = _ctx.Users
                .Include("Roles")
                .OrderBy(u => u.UserName)
                .Where(u => u.Roles.Any(r => r.RoleName == "Administrator"))
                .Select(u => new AdminUserModel()
                                 {
                                     EmailAddress = u.EmailAddress,
                                     Id = u.UserId,
                                     UserName = u.UserName,
                                     CreateDate = u.CreateDate,
                                     LastLoggedIn = u.LastLoggedIn,
                                     IsLockedOut = u.IsLockedOut
                                 });

            return View(model.ToPagedList(pageIndex, pageSize));
        }
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}