using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class MeCodesModels
    {
        public class MeCodesModel
        {
            public List<UserModels.UserPayPointResponse> PayPoints { get; set; }
        }
        public class AddMeCodeModel
        {
            public String MeCode { get; set; }
        }
        public class DetailsMeCodeModel
        {
            public UserModels.UserPayPointResponse MeCode { get; set; }
        }
        public class DeleteMeCodeModel
        {
            public String PayPointId { get; set; }
        }
    }
}