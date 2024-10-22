﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Services;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Security;
using System.Web.Routing;
using System.Configuration;
using System.Text;
using Mobile_PaidThx.CustomAttributes;


namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class SocialNetworksController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        public ActionResult Index()
        {
            var userService = new Services.UserServices();
            
            var user = userService.GetUser(Session["UserId"].ToString());

            Session["FBState"] = "pickle";

            return View(new SocialNetworksModels.Index()
            {
                FBState = "pickle",
                UserSocialNetworks = user.userSocialNetworks.Select(u => new SocialNetworksModels.UserSocialNetwork() {
                    Name = u.SocialNetwork
                }).ToList()
            });
        }
        public ActionResult LinkFacebookAccount(string state, string code)
        {
            if (Session["FBState"] == null)
                  return RedirectToAction("Index");

            var userService = new Services.UserServices();
            var faceBookServices = new FacebookServices();
            var userSocialNetworkServices = new UserSocialNetworkServices();
            string fbState = Session["FBState"].ToString();

            var user = userService.GetUser(Session["UserId"].ToString());

            var redirect = String.Format(fbTokenRedirectURL, "SocialNetworks/LinkFacebookAccount");

            if (state != fbState)
                throw new Exception("Unable to Link Facebook Account.  Invalid State");

            var fbAccount = faceBookServices.FBauth(code, redirect);

            UserModels.FacebookSignInResponse facebookSignInResponse;
            bool isNewUser = false;

            try
            {
                userSocialNetworkServices.LinkSocialNetworkAccount(user.userId.ToString(), "Facebook", fbAccount.id, fbAccount.accessToken);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Signing In with Facebook. {0} Stack Trace: {1}", ex.Message, ex.StackTrace));

                TempData["Message"] = ex.Message;

                return RedirectToAction("Index");
            }

            List<FacebookModels.Friend> friends;

            try
            {
                _logger.Log(LogLevel.Error, String.Format("Getting Facebook Friends. Access Token {0}", fbAccount.accessToken));
            
                friends = faceBookServices.GetFriendsList(fbAccount.accessToken);
            }
            catch (Exception ex)
            {
                friends = new List<FacebookModels.Friend>();

                _logger.Log(LogLevel.Error, String.Format("Exception Getting Facebook Friends. Access Token {0}. Exception: {1}", fbAccount.accessToken, ex.Message));
            }

            Session["Friends"] = friends;

            return RedirectToAction("Index");
            
        }
        public ActionResult UnlinkFacebookAccount()
        {
            var userService = new Services.UserServices();
            var faceBookServices = new FacebookServices();
            var userSocialNetworkServices = new UserSocialNetworkServices();

            var user = userService.GetUser(Session["UserId"].ToString());

            try
            {
                userSocialNetworkServices.RemoveLinkedSocialNetworkAccount("Facebook", user.userId.ToString());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Unlinking Facebook Account for User {0}. Exception: {1}", user.userId.ToString(), ex.Message));
           
            }

            var socialNetwork = user.userSocialNetworks.FirstOrDefault(u => u.SocialNetwork == "Facebook");
            user.userSocialNetworks.Remove(socialNetwork);

            Session["Friends"] = null;

            return RedirectToAction("Index");
        }
    }
}
