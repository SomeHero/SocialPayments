using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class PreferencesModels
    {
        public class ProfileModel
        {
            public List<ProfileSectionResponse> ProfileSections { get; set; }
        }
        
        public class SocialNetworks
        {

        }
        public class MeCodesModel
        {
            public List<UserModels.UserPayPointResponse> PayPoints { get; set; }
        }
        public class NotificationsModel
        {

        }
        public class SharingModel
        {

        }

    }
}