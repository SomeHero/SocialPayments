using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using System.Web.Routing;

namespace Mobile_PaidThx.Controllers
{
    public class DoGoodController : Controller
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /DoGood/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /DoGood/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult PinswipeDonate(Mobile_PaidThx.Models.DonateMoneyModel model)
        {
            return View(model);
        }


        [HttpPost]
        public ActionResult DonateMoney(DonateMoneyModel model)
        {
            logger.Log(LogLevel.Debug, String.Format("Payment Request Posted to {0} of {1} with Comments {2}", model.Organization, model.Amount, model.Comments));

            var applicationService = new SocialPayments.DomainServices.ApplicationService();
            var userId = Session["UserId"].ToString();

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            logger.Log(LogLevel.Debug, String.Format("Found user and payment account"));

            if (ModelState.IsValid)
            {
                try
                {

                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
                    var paystreamMessageServices = new PaystreamMessageServices();
                    var response = paystreamMessageServices.SendDonation(_apiKey, userId, "", user.userName, user.preferredPaymentAccountId, model.Organization, model.Pincode, model.Amount, model.Comments, "Payment", "0", "0", "", "", "");
                    //ctx.PaymentRequests.Add(new PaymentRequest()
                    //{
                    //    Amount = model.Amount,
                    //    ApiKey = application.ApiKey,
                    //    Comments = model.Comments,
                    //    CreateDate = System.DateTime.Now,
                    //    PaymentRequestId = Guid.NewGuid(),
                    //    PaymentRequestStatus = PaymentRequestStatus.Submitted,
                    //    RecipientUri = model.RecipientUri,
                    //    RequestorId = user.UserId
                    //});
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request. {0}", ex.Message));

                    return View(model);
                }
            }
            else
                return View(model);

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }

        public ActionResult AddOrg()
        {
            Mobile_PaidThx.Models.OrganizationModels.Organizations orgs = new Models.OrganizationModels.Organizations();
            orgs.NonProfits = new List<Models.OrganizationModels.OrganizationModel>();
            orgs.PublicDirectories = new List<Models.OrganizationModels.OrganizationModel>();
            // add to non-profts
            // pre-created list for testing


            // NON-PROFITS
            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel acs = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "1",
                Name = "American Cancer Society",
                Slogan = "The official sponsor of birthdays",
                ImageUri = "~/Content/images/org_acs.png",
                HasInfo = true
            };
            orgs.NonProfits.Add(acs);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel ad = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "2",
                Name = "American Diabetes Association",
                Slogan = "",
                ImageUri = "~/Content/images/org_americandiabetes.png",
                HasInfo = false
            };

            orgs.NonProfits.Add(ad);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel aheart = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "3",
                Name = "American Heart Association",
                Slogan = "Learn and Live",
                ImageUri = "~/Content/images/org_americanheart.png",
                HasInfo = false
            };

            orgs.NonProfits.Add(aheart);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel cs = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "4",
                Name = "Child Savers",
                Slogan = "Helping Greater Richmond's Children since 1924",
                ImageUri = "~/Content/images/org_childsavers.png",
                HasInfo = true
            };

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel gw = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "5",
                Name = "Goodwill",
                Slogan = "",
                ImageUri = "~/Content/images/org_goodwill.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(cs);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel md = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "6",
                Name = "March of Dimes",
                Slogan = "Working together for stronger, healther babies",
                ImageUri = "~/Content/images/org_marchofdimes.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(md);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel mda = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "7",
                Name = "Muscle Dystrophy Association",
                Slogan = "Fighting muscle disease",
                ImageUri = "~/Content/images/org_mda.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(mda);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel nc = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "8",
                Name = "Nature Conservancy",
                Slogan = "Protecting Nature. Perserving Life",
                ImageUri = "~/Content/images/org_natureconserv.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(nc);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel wwf = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "9",
                Name = "World Wildlife Foundation",
                Slogan = "",
                ImageUri = "~/Content/images/org_wwf.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(wwf);


            // PUBLIC DIRECTORIES
            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel beta = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Beta Theta Pi",
                Slogan = "Men of principle",
                ImageUri = "~/Content/images/org_beta.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(beta);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel bsa = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Boy Scouts of America",
                Slogan = "",
                ImageUri = "~/Content/images/org_bsa.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(bsa);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel cvsa = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Central Virginia Soccer Association",
                Slogan = "35+ Years of the Best Adult Soccer in the Richmond Area",
                ImageUri = "~/Content/images/org_cvsa.png",
                HasInfo = false
            };

            orgs.PublicDirectories.Add(cvsa);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel hi = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Hostelling International",
                Slogan = "Travel with a Mission",
                ImageUri = "~/Content/images/org_hostelling.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(hi);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel obx = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Holy Redeemer Catholic Parish",
                Slogan = "",
                ImageUri = "~/Content/images/org_obxparish.png",
                HasInfo = false
            };

            orgs.PublicDirectories.Add(obx);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel rs = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "River City Sports",
                Slogan = "The Premier Name for Sports Merchandise",
                ImageUri = "~/Content/images/org_rivercitysports.png",
                HasInfo = false
            };

            orgs.PublicDirectories.Add(rs);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel ri = new Models.OrganizationModels.OrganizationModel()
            {

                Name = "Rotary International",
                Slogan = "Looking into the future",
                ImageUri = "~/Content/images/org_rotary.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(ri);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel so = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Special Olympics",
                Slogan = "",
                ImageUri = "~/Content/images/org_specialolympics.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(so);
            return View(orgs);
        }

        public ActionResult AddToSend()
        {
            return View();
        }

        public ActionResult AddOrgPledge()
        {
            Mobile_PaidThx.Models.OrganizationModels.Organizations orgs = new Models.OrganizationModels.Organizations();
            orgs.NonProfits = new List<Models.OrganizationModels.OrganizationModel>();
            orgs.PublicDirectories = new List<Models.OrganizationModels.OrganizationModel>();
            // add to non-profts
            // pre-created list for testing


            // NON-PROFITS
            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel acs = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "1",
                Name = "American Cancer Society",
                Slogan = "The official sponsor of birthdays",
                ImageUri = "~/Content/images/org_acs.png",
                HasInfo = true
            };
            orgs.NonProfits.Add(acs);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel ad = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "2",
                Name = "American Diabetes Association",
                Slogan = "",
                ImageUri = "~/Content/images/org_americandiabetes.png",
                HasInfo = false
            };

            orgs.NonProfits.Add(ad);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel aheart = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "3",
                Name = "American Heart Association",
                Slogan = "Learn and Live",
                ImageUri = "~/Content/images/org_americanheart.png",
                HasInfo = false
            };

            orgs.NonProfits.Add(aheart);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel cs = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "4",
                Name = "Child Savers",
                Slogan = "Helping Greater Richmond's Children since 1924",
                ImageUri = "~/Content/images/org_childsavers.png",
                HasInfo = true
            };

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel gw = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "5",
                Name = "Goodwill",
                Slogan = "",
                ImageUri = "~/Content/images/org_goodwill.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(cs);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel md = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "6",
                Name = "March of Dimes",
                Slogan = "Working together for stronger, healther babies",
                ImageUri = "~/Content/images/org_marchofdimes.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(md);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel mda = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "7",
                Name = "Muscle Dystrophy Association",
                Slogan = "Fighting muscle disease",
                ImageUri = "~/Content/images/org_mda.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(mda);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel nc = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "8",
                Name = "Nature Conservancy",
                Slogan = "Protecting Nature. Perserving Life",
                ImageUri = "~/Content/images/org_natureconserv.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(nc);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel wwf = new Models.OrganizationModels.OrganizationModel()
            {
                Id = "9",
                Name = "World Wildlife Foundation",
                Slogan = "",
                ImageUri = "~/Content/images/org_wwf.png",
                HasInfo = true
            };

            orgs.NonProfits.Add(wwf);


            // PUBLIC DIRECTORIES
            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel beta = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Beta Theta Pi",
                Slogan = "Men of principle",
                ImageUri = "~/Content/images/org_beta.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(beta);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel bsa = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Boy Scouts of America",
                Slogan = "",
                ImageUri = "~/Content/images/org_bsa.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(bsa);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel cvsa = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Central Virginia Soccer Association",
                Slogan = "35+ Years of the Best Adult Soccer in the Richmond Area",
                ImageUri = "~/Content/images/org_cvsa.png",
                HasInfo = false
            };

            orgs.PublicDirectories.Add(cvsa);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel hi = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Hostelling International",
                Slogan = "Travel with a Mission",
                ImageUri = "~/Content/images/org_hostelling.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(hi);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel obx = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Holy Redeemer Catholic Parish",
                Slogan = "",
                ImageUri = "~/Content/images/org_obxparish.png",
                HasInfo = false
            };

            orgs.PublicDirectories.Add(obx);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel rs = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "River City Sports",
                Slogan = "The Premier Name for Sports Merchandise",
                ImageUri = "~/Content/images/org_rivercitysports.png",
                HasInfo = false
            };

            orgs.PublicDirectories.Add(rs);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel ri = new Models.OrganizationModels.OrganizationModel()
            {

                Name = "Rotary International",
                Slogan = "Looking into the future",
                ImageUri = "~/Content/images/org_rotary.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(ri);

            Mobile_PaidThx.Models.OrganizationModels.OrganizationModel so = new Models.OrganizationModels.OrganizationModel()
            {
                Name = "Special Olympics",
                Slogan = "",
                ImageUri = "~/Content/images/org_specialolympics.png",
                HasInfo = true
            };

            orgs.PublicDirectories.Add(so);
            return View(orgs);
        }

        public ActionResult AmountToDonate()
        {
            return View();
        }

        public ActionResult AmountToPledge()
        {
            return View();
        }

        public ActionResult Donate()
        {
            Mobile_PaidThx.Models.DonateMoneyModel nullmodel = new Models.DonateMoneyModel();
            return View(nullmodel);
        }

        [HttpPut]
        public ActionResult Donate(Mobile_PaidThx.Models.DonateMoneyModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult DonateData(Mobile_PaidThx.Models.DonateMoneyModel model)
        {
            return Json(model);
        }

        public ActionResult Pledge()
        {
            Mobile_PaidThx.Models.DonateMoneyModel nullmodel = new Models.DonateMoneyModel();
            return View(nullmodel);
        }

        [HttpPut]
        public ActionResult Pledge(Mobile_PaidThx.Models.DonateMoneyModel model)
        {
            return View(model);
        }

        //
        // GET: /DoGood/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DoGood/Create

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
        // GET: /DoGood/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /DoGood/Edit/5

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
        // GET: /DoGood/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DoGood/Delete/5

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
