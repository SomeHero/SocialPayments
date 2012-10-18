using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Controllers.Base;
using System.Configuration;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using System.Web.Routing;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.CustomAttributes;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class ProfileController : PaidThxBaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying Profile View"));

            var user = (UserModels.UserResponse)Session["User"];
            var application = (ApplicationResponse)Session["Application"];

            return View(new PreferencesModels.ProfileModel()
            {
                ProfileSections = application.ProfileSections,
                User = user
            });
        }
        [HttpPost]
        public void Index(UpdateProfileModel model)
        {
            logger.Log(LogLevel.Info, String.Format("Updating Profile {0}:{1}", model.Key, model.Value));
            
            var userAttributeServices = new UserAttributeServices();
            var user = (UserModels.UserResponse)Session["User"];

            try
            {
                userAttributeServices.UpdateConfigurationSetting(user.userId.ToString(), model.Key, model.Value);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Exception Updating User Configuration Variable. Exception: {0}", ex.Message));
            }

            Guid attributeGuid;
            Guid.TryParse(model.Key, out attributeGuid);

            var userAttribute = user.userAttributes.FirstOrDefault(a => a.AttributeId == attributeGuid);

            if (userAttribute == null)
            {
                user.userAttributes.Add(new UserModels.UserAttribute()
                {
                    AttributeId = attributeGuid,
                    AttributeValue = model.Value
                });
            }
            else
                userAttribute.AttributeValue = model.Value;
        }
    }
}
