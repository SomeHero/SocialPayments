﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Models;
using System.Configuration;
using NLog;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Text;

namespace Mobile_PaidThx.Services
{
    public class FacebookServices
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        private string fbAppID = "332189543469634";
        private string fbAppSecret = "628b100a8e6e9fd8278406a4a675ce0c";
        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        public FacebookUserModels.FBuser FBauth(string code, string redirect_uri)
        {
            if (redirect_uri.Substring(redirect_uri.Length - 1) != "/")
                redirect_uri = redirect_uri + "/";

            _logger.Log(LogLevel.Debug, String.Format("FBAuth called  Code {0}, redirect_uri: {1}", code, redirect_uri));

            string response = null;
            string token = null;
            string tokenExp = null;
            FacebookUserModels.FBuser fbAccount = new FacebookUserModels.FBuser();

                //Exchange FB Code for FB Token
                string requestToken = "https://graph.facebook.com/oauth/access_token?" +
                    "client_id=" + fbAppID +
                    "&redirect_uri=" + redirect_uri +
                    "&client_secret=" + fbAppSecret +
                    "&code=" + code;

                _logger.Log(LogLevel.Info, requestToken);
                HttpWebRequest wr = GetWebRequest(requestToken);
                HttpWebResponse resp = null;

                try
                {
                    resp = (HttpWebResponse)wr.GetResponse();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Info, ex.Message);
                }

                if (resp == null)
                    throw new Exception("Facebook response was null");

                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    response = sr.ReadToEnd();


                    if (response.Length > 0)
                    {
                        _logger.Log(LogLevel.Info, response);
                        NameValueCollection qs = HttpUtility.ParseQueryString(response);

                        if (qs["access_token"] != null)
                        {
                            token = qs["access_token"];
                            tokenExp = qs["expires"];
                        }
                    }
                    sr.Close();
                }

                fbAccount.accessToken = token;
                fbAccount.tokenExpires = tokenExp;

                _logger.Log(LogLevel.Info, String.Format("Facebook token {0}", token));

                //Use Graph API to get FB UserID and email
                string requestStuff = "https://graph.facebook.com/me?access_token=" + token;
                wr = GetWebRequest(requestStuff);
                resp = (HttpWebResponse)wr.GetResponse();

                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    response = sr.ReadToEnd();

                    _logger.Log(LogLevel.Info, String.Format("Facebook response {0}", response));

                    if (response.Length > 0)
                    {
                        fbAccount = JsonConvert.DeserializeObject<FacebookUserModels.FBuser>(response);
                        fbAccount.accessToken = token;
                        fbAccount.tokenExpires = tokenExp;

                    }
                    sr.Close();
                }
            

            return fbAccount;
        }
        public List<FacebookModels.Friend> GetFriendsList(string token)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting Friends List for Token {0}", token));

            //Use Graph API to get FB UserID and email
            string requestStuff = "https://graph.facebook.com/me/friends?access_token=" + token;
            string response = null;
            HttpWebRequest wr = null;
            HttpWebResponse resp = null;
            Dictionary<String, List<FacebookModels.Friend>> friends = null;
            var jsonSerializer = new JavaScriptSerializer();

            wr = GetWebRequest(requestStuff);
            resp = (HttpWebResponse)wr.GetResponse();

            if (resp == null)
                throw new Exception(String.Format("Facebook response was null"));

            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                response = sr.ReadToEnd();
                if (response.Length > 0)
                {
                    friends = jsonSerializer.Deserialize<Dictionary<String, List<FacebookModels.Friend>>>(response);
                }
                sr.Close();
            }

            return friends["data"];
        }

        private HttpWebRequest GetWebRequest(string formattedUri)
        {
            _logger.Log(LogLevel.Info, String.Format("Get Web Request {0}", formattedUri));

            // Create the request’s URI.
            Uri serviceUri = new Uri(formattedUri, UriKind.Absolute);

            // Return the HttpWebRequest.
            return (HttpWebRequest)WebRequest.Create(serviceUri);
        }


    }
}