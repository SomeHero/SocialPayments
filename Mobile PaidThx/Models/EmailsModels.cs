using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class EmailsModels
    {
        public class EmailsModel
        {
            public List<UserModels.UserPayPointResponse> PayPoints { get; set; }
        }
        public class AddEmailAddressModel
        {
            public String EmailAddress { get; set; }
        }
        public class DetailsEmailModel
        {
            public UserModels.UserPayPointResponse EmailAddress { get; set; }
        }
        public class DeleteEmailModel
        {
            public String PayPointId { get; set; }
        }
        public class ResendEmailVerificationModel
        {
            public String PayPointId { get; set; }
        }
    }
}