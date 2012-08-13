﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;

namespace Mobile_PaidThx.Services
{
    public class ServicesBase
    {
        protected string Get(string serviceUrl)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(serviceUrl);
            req.Method = "GET";

            HttpStatusCode statusCode = HttpStatusCode.OK;

            string statusReason = String.Empty;
            string jsonResponse = String.Empty;

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)req.GetResponse();

                statusCode = httpResponse.StatusCode;
                statusReason = httpResponse.StatusDescription;

                using (StreamReader sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    jsonResponse = sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;

                    statusCode = httpResponse.StatusCode;
                    statusReason = httpResponse.StatusDescription;

                }
            }

            if (statusCode.Equals(HttpStatusCode.BadRequest))
            {
                throw new Exception(statusReason);
            }

            return jsonResponse;
        }
        protected string Post(string serviceUrl, string json)
        {
            // Create new HTTP request.
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(serviceUrl);
            req.Method = "POST";
            req.ContentType = "application/json";
            byte[] postData = Encoding.ASCII.GetBytes(json);
            req.ContentLength = postData.Length;

            // Send HTTP request.
            Stream PostStream = req.GetRequestStream();
            PostStream.Write(postData, 0, postData.Length);
            HttpStatusCode statusCode = HttpStatusCode.OK;

            string statusReason = String.Empty;
            string jsonResponse = String.Empty;

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)req.GetResponse();

                statusCode = httpResponse.StatusCode;
                statusReason = httpResponse.StatusDescription;

                using (StreamReader sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    jsonResponse = sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;

                    statusCode = httpResponse.StatusCode;
                    statusReason = httpResponse.StatusDescription;

                }
            }

            if (statusCode.Equals(HttpStatusCode.BadRequest))
            {
                throw new Exception(statusReason);
            }

            return jsonResponse;
        }
    }
}