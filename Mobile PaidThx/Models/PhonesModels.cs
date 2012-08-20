using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class PhonesModels
    {
        public class PhonesModel
        {
            public List<UserModels.UserPayPointResponse> PayPoints { get; set; }
        }
        public class AddPhoneModel
        {
            public String PhoneNumber { get; set; }
        }
        public class DetailsPhonesModels
        {
            public UserModels.UserPayPointResponse PhoneNumber { get; set; }
        }
        public class DeletePhonesModel
        {
            public String PayPointId { get; set; }
        }
    }
}