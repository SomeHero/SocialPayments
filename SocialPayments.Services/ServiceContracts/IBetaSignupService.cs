using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using SocialPayments.Services.DataContracts.BetaSignUp;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IBetaSignupService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/BetaSignups", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        BetaSignupResponse AddBetaSignUp(BetaSignUpRequest request);

    }
}