using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SocialPayments.DataLayer;
using SocialPayments.Web.Admin.Models;

namespace SocialPayments.Web.Admin.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        private readonly Context _ctx = new Context();

        public ActionResult Index(int pageIndex, int pageSize)
        {
            var model = _ctx.Users
                .Include("Roles")
                .OrderByDescending(u => u.CreateDate)
                .Where(u => u.Roles.Any(r => r.RoleName == "Member"))
                .Select(u => new UserModel()
                                 {
                                     MobileNumber = u.MobileNumber,
                                     ConfirmedDate = System.DateTime.Now,
                                     EmailAddress = u.EmailAddress,
                                     Id = u.UserId,
                                     IsConfirmed = u.IsConfirmed,
                                     SignUpDate = u.CreateDate,
                                     UserName = u.UserName
                                 });

            return View(model.ToPagedList(pageIndex, pageSize));
        }

    }
}
