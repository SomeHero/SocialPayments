﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class UserPayPointServices: ServicesBase
    {
        private string _getPayPointsUrl = "{0}Users/{1}/paypoints";
        private string _deletePaypointUrl = "{0}Users/{1}/paypoints/{2}";
        private string _addPaypointUrl = "{0}Users/{1}/paypoints";
        private string _resendEmailVerificationLinkUrl = "{0}Users/{1}/PayPoints/resend_email_verification_link";
        private string _resendPhoneVerificationCodeUrl = "{0}Users/{1}/PayPoints/resend_verification_code";

        public List<UserModels.UserPayPointResponse> GetPayPoints(string userId)
        {
            var serviceUrl = String.Format(_getPayPointsUrl, _webServicesBaseUrl, userId);

            var response = Get(serviceUrl);
            JavaScriptSerializer js = new JavaScriptSerializer();

            if(response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<List<UserModels.UserPayPointResponse>>(response.JsonResponse);
        }
        public void AddPaypoint(String userId, String uri, String type)
        {
            var serviceUrl = String.Format(_addPaypointUrl, _webServicesBaseUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                Uri = uri,
                PayPointType = type
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

        }


        public void DeletePaypoint(string apiKey, string userId, string paypointId)
        {
            string serviceUrl = String.Format(_deletePaypointUrl, _webServicesBaseUrl, userId, paypointId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var response = Delete(serviceUrl, "");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
        public void ResendEmailVerificationLink(String userId, String payPointId)
        {
            var serviceUrl = String.Format(_resendEmailVerificationLinkUrl, _webServicesBaseUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                UserPayPointId = payPointId
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

        }
        public void ResendPhoneVerificationCode(String userId, String payPointId)
        {
            var serviceUrl = String.Format(_resendPhoneVerificationCodeUrl, _webServicesBaseUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                UserPayPointId = payPointId
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

        }
    }
}