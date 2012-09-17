using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
using NLog;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class FacebookMessageController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string _facebookShareMessageTemplate = "I sent money to my friend using PaidThx. It was fast, easy, and FREE! Check it out. www.PaidThx.com";
        private string _facebookFriendWallPostLinkTitleTemplate = "Check out PaidThx!";
        private string _facebookFriendWallPostPictureURL = "http://www.crunchbase.com/assets/images/resized/0019/7057/197057v2-max-250x250.png";
        private string _facebookFriendWallPostDescription = "PaidThx lets you send and receive money whenever you want, wherever you want. It is easy to do, and doesn't cost you a penny.";
        private string _facebookFriendWallPostCaption = "The FREE Social Payment Network";

        //POST /api/{userId}/share_on_facebook
        public HttpResponseMessage PostShareMessage(string id, FBMessageModel.ShareRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                IAmazonNotificationService amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.MessageServices _messageServices = new DomainServices.MessageServices(_ctx, amazonNotificationService);

                User user;

                // Validate that it finds a user
                user = _userService.GetUserById(id);

                if (user == null)
                {
                    var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    errorMessage.ReasonPhrase = String.Format("The user's account cannot be found for user {0}", id);

                    return errorMessage;
                }

                if (request.TransactionId == null || String.IsNullOrEmpty(request.TransactionId))
                {
                    var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    errorMessage.ReasonPhrase = String.Format("No transactionID passed for Facebook Share Request for user {0}", id);

                    return errorMessage;
                }

                if (request.WallMessage == null || String.IsNullOrEmpty(request.WallMessage))
                {
                    var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    errorMessage.ReasonPhrase = String.Format("No wallMessage passed for Facebook Share Request for user {0}", id);

                    return errorMessage;
                }

                if (user.FacebookUser == null || String.IsNullOrEmpty(user.FacebookUser.OAuthToken))
                {
                    var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    errorMessage.ReasonPhrase = String.Format("No valid facebook session found for Facebook Share Request for user {0}", id);

                    return errorMessage;
                }
                else
                {
                    Domain.Message message = null;

                    try
                    {
                        message = _messageServices.GetMessage(id);

                    }
                    catch (Exception ex)
                    {
                        var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        errorMessage.ReasonPhrase = String.Format("Unhandled Exception Getting Message {0}. {1}", id, ex.Message);

                        return errorMessage;
                    }

                    try
                    {
                        var client = new Facebook.FacebookClient(message.Sender.FacebookUser.OAuthToken);
                        var args = new Dictionary<string, object>();

                        args["message"] = _facebookShareMessageTemplate;
                        args["link"] = message.shortUrl;

                        args["name"] = _facebookFriendWallPostLinkTitleTemplate;
                        args["caption"] = _facebookFriendWallPostCaption;
                        args["picture"] = _facebookFriendWallPostPictureURL;
                        args["description"] = _facebookFriendWallPostDescription;

                        client.Post("feed", args);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, ex.Message);

                        var errorMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        errorMessage.ReasonPhrase = String.Format("Unhandled Exception Sharing on Facebook {0}. {1}", id, ex.Message);

                        return errorMessage;
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
        }
    }
}
