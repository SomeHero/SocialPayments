using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.CustomAttributes;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class SharingController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var subjectKeys = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("ReceiveMoney", "When I Receive Money"),
                new KeyValuePair<string, string>("ReceiveRequest", "When I Request a Request"),
                new KeyValuePair<string, string>("RequestMoney", "When I Request Money"),
                new KeyValuePair<string, string>("SendMoney", "When I Send Money")
            };

            var configTypes = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Twitter", " Twitter"),
                new KeyValuePair<string, string>("Facebook", "Facebook")
            };

            var configSubjects = new List<NotificationModels.NotificationSubject>();

            foreach (var subject in subjectKeys)
            {
                var configItems = new List<NotificationModels.NotificationItem>();

                foreach (var type in configTypes)
                {
                    var configKey = String.Format("Sharing_{0}_{1}", subject.Key, type.Key);
                    var configValue = "0";

                    var configItem = user.userConfigurationVariables.FirstOrDefault(c => c.ConfigurationKey == configKey);
                    if (configItem != null)
                    {
                        if (Convert.ToBoolean(configItem.ConfigurationValue))
                            configValue = "1";
                    }

                    configItems.Add(new NotificationModels.NotificationItem()
                    {
                        ConfigurationKey = configKey,
                        Description = type.Value,
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
                    Description = subject.Value,
                    NotificationItems = configItems
                });

            }

            return View(new NotificationModels.NotificationModel()
            {
                NotificationSubjects = configSubjects
            });
        }
        [HttpPost]
        public void Index(string key, string value)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var userConfigurationService = new UserConfigurationServices();

            var configValue = "false";
            if (value == "1")
                configValue = "true";

            var configItem = user.userConfigurationVariables.FirstOrDefault(c => c.ConfigurationKey == key);
            if (configItem != null)
            {
                configItem.ConfigurationValue = configValue;
            }
            else
            {
                user.userConfigurationVariables.Add(new UserModels.UserConfigurationResponse()
                {
                    ConfigurationKey = key,
                    ConfigurationValue = configValue,
                    UserId = user.userId.ToString()
                });
            }

            try
            {
                userConfigurationService.UpdateConfigurationSetting(user.userId.ToString(), key, configValue);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Updating User Configuration Variable. Exception: {0}", ex.Message));
            }
        }
    }
}
