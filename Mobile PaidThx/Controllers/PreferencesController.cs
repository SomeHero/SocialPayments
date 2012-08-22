using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Models;
using System.Web.Security;
using NLog;

namespace Mobile_PaidThx.Controllers
{
    public class PreferencesController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /Preferences/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Profile()
        {
            ApplicationResponse application = (ApplicationResponse)Session["Application"];

            return View(new PreferencesModels.ProfileModel()
            {
                ProfileSections = application.ProfileSections
            });
        }

        public ActionResult Help()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ReturnData(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            return Json(model);
        }

        [HttpPut]
        public ActionResult PaypointInfo(Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse model)
        {
            return View(model);
        }

        public ActionResult SocialNetworks()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            List<Mobile_PaidThx.Services.ResponseModels.UserModels.UserPayPointResponse> listOfPaypoints = user.userPayPoints.Where(p => p.Type.Equals("SocialNetwork")).ToList();
            return View(listOfPaypoints);
        }

        public ActionResult Notifications()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var subjectKeys = new List<string>()
            {
                "ReceiveMoney",
                "SendMoney",
                "ReceiveRequest",
                "SendRequest"
            };

            var configTypes = new List<string>()
            {
                "EmailMessage",
                "PushNotification",
                "TextMessage"
            };

            var configSubjects = new List<NotificationModels.NotificationSubject>();

            foreach (var subject in subjectKeys)
            {
                var configItems = new List<NotificationModels.NotificationItem>();

                foreach (var type in configTypes)
                {
                    var configKey = String.Format("Notification_{0}_{1}", subject, type);
                    var configValue = "Off";

                    var configItem = user.userConfigurationVariables.FirstOrDefault(c => c.ConfigurationKey == configKey);
                    if (configItem != null)
                    {
                        if (Convert.ToBoolean(configItem.ConfigurationValue))
                            configValue = "On";
                    }

                    configItems.Add(new NotificationModels.NotificationItem()
                    {
                        ConfigurationKey = configKey,
                        Description = type,
                        On = true,
                        Options = new List<KeyValuePair<string, string>>() {
                                    new KeyValuePair<string, string>("On", "1"),
                                    new KeyValuePair<string, string>("Off", "0")
                                },
                        SelectedValue = configValue.ToString()
                    });

                }

                configSubjects.Add(new NotificationModels.NotificationSubject()
                {
                    Description = subject,
                    NotificationItems = configItems
                });

            }

            return View(new NotificationModels.NotificationModel()
            {
                NotificationSubjects = configSubjects
            });
        }
        [HttpPost]
        public void Notifications(string key, string value)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var userConfigurationService = new UserConfigurationServices();

            try
            {
                userConfigurationService.UpdateConfigurationSetting(user.userId.ToString(), key, value);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Updating User Configuration Variable. Exception: {0}", ex.Message));
            }
        }
        public ActionResult Sharing()
        {
            return View(new SharingModels.SharingModel()
            {
                SharingSubjects = new List<SharingModels.SharingSubject>()
                {
                    new SharingModels.SharingSubject() {
                        Description = "When I Receive Money",
                        SharingItems = new List<SharingModels.SharingItem>() {
                            new SharingModels.SharingItem() {
                                Description = "Facebook",
                                On = false,
                                UserConfigurationId = ""
                            },
                            new SharingModels.SharingItem() {
                                Description = "Twitter",
                                On = false,
                                UserConfigurationId = ""
                            },
                        }
                    },
                    new SharingModels.SharingSubject() {
                        Description = "When I Receive a Request",
                        SharingItems = new List<SharingModels.SharingItem>() {
                            new SharingModels.SharingItem() {
                                Description = "Facebook",
                                On = false,
                                UserConfigurationId = ""
                            },
                            new SharingModels.SharingItem() {
                                Description = "Twitter",
                                On = false,
                                UserConfigurationId = ""
                            },
                        }
                    },
                     new SharingModels.SharingSubject() {
                        Description = "When I Request Money",
                        SharingItems = new List<SharingModels.SharingItem>() {
                            new SharingModels.SharingItem() {
                                Description = "Facebook",
                                On = false,
                                UserConfigurationId = ""
                            },
                            new SharingModels.SharingItem() {
                                Description = "Twitter",
                                On = false,
                                UserConfigurationId = ""
                            },
                        }
                    },
                      new SharingModels.SharingSubject() {
                        Description = "When I Send Money",
                        SharingItems = new List<SharingModels.SharingItem>() {
                            new SharingModels.SharingItem() {
                                Description = "Facebook",
                                On = false,
                                UserConfigurationId = ""
                            },
                            new SharingModels.SharingItem() {
                                Description = "Twitter",
                                On = false,
                                UserConfigurationId = ""
                            },
                        }
                    }
                }
            });
        }

        public ActionResult Security()
        {
            return View();
        }
        public ActionResult UserAgreement()
        {
            return View();
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "SignIn", new System.Web.Routing.RouteValueDictionary() { });
        }

        //
        // GET: /Preferences/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Preferences/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Preferences/Create

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
        // GET: /Preferences/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Preferences/Edit/5

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
        // GET: /Preferences/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Preferences/Delete/5

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
