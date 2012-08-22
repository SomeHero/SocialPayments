using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.ResponseModels
{
    public class UserConfigurationModels
    {
        public class UpdateUserConfigurationRequest
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}