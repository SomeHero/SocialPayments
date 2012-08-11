using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;
using NLog;

namespace SocialPayments.DomainServices
{
    public class FacebookServices
    {
        private IDbContext _ctx;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private string _facebookFriendWallPostLinkTitleTemplate = "You were sent money!";
        private string _facebookFriendWallPostPictureURL = "http://www.crunchbase.com/assets/images/resized/0019/7057/197057v2-max-250x250.png";
        private string _facebookFriendWallPostDescription = "PaidThx lets you send and receive money whenever you want, wherever you want. It is easy to do, and doesn't cost you a penny.";
        private string _facebookFriendWallPostCaption = "The FREE Social Payment Network";

        public FacebookServices()
            : this(new Context())
        { }

        public FacebookServices(IDbContext context)
        {
            _ctx = context;
        }

        public void MakeWallPost(string oAuthToken, string recipientFacebookId, string message, string link)
        {
            _logger.Log(LogLevel.Info, String.Format("Posting to facebook wall for {0}", recipientFacebookId));

            var client = new Facebook.FacebookClient(oAuthToken);
            var args = new Dictionary<string, object>();

            args["message"] = message;
            args["link"] = link;

            args["name"] = _facebookFriendWallPostLinkTitleTemplate;
            args["caption"] = _facebookFriendWallPostCaption;
            args["picture"] = _facebookFriendWallPostPictureURL;
            args["description"] = _facebookFriendWallPostDescription;

            client.Post(String.Format("/{0}/feed", recipientFacebookId), args);
        }
    }
}
