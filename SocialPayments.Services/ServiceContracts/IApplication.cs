using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using SocialPayments.Services.DataContracts.Application;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IApplicationService
    {
        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/Applications", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        ApplicationResponse AddApplication(ApplicationRequest applicationRequest);

        [OperationContract]
        [WebGet(UriTemplate = "/Applications", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        List<ApplicationResponse> GetApplications();

        [OperationContract]
        [WebGet(UriTemplate = "/Applications/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        ApplicationResponse GetApplication(string id);

        [OperationContract]
        [WebInvoke(Method = "Put", UriTemplate = "/Applications", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        void UpdateApplication(ApplicationRequest applicationRequest);

        [OperationContract]
        [WebInvoke(Method = "Delete", UriTemplate = "/Applications", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        void DeleteApplication(ApplicationRequest calendarRequest);
    }
}