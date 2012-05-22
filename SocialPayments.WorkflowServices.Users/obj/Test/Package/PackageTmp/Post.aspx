<%@ Page Language="C#" ValidateRequest="false" %>
<%@ Import Namespace="SocialPayments.DataLayer" %>
<%@ Import Namespace="SocialPayments.DataLayer.Interfaces" %>
<%@ Import Namespace="SocialPayments.DomainServices" %>
<%@ Import Namespace="NLog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {

        
        String AWSAccessKeyId = "AKIAIKSNRNLLIFLI7AMA";
        String AWSSecretAccessKey = "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";

        String EmailFromAddress = "jrhodes2705@gmail.com";
        String EmailToAddress = "jrhodes2705@gmail.com";
        String EmailSmtpHost = "smtp.gmail.com";
        int EmailSmtpPort = 587;
        Boolean EmailSmtpEnableSsl = true;
        String EmailSmtpUserName = "jrhodes2705@gmail.com";
        String EmailSmtpPassword = "Sebast1an10";

        Boolean ValidateSignature = true;
        Boolean IsSignatureValid = false;

        
        //The LogMessage will contain information about what the script did.  It will be sent through email at the end.
        String LogMessage = "";
        LogMessage += "DateTime = " + DateTime.Now + Environment.NewLine;
        LogMessage += "SERVER_NAME = " + Request.ServerVariables["SERVER_NAME"] + Environment.NewLine;
        LogMessage += "LOCAL_ADDR = " + Request.ServerVariables["LOCAL_ADDR"] + Environment.NewLine;
        LogMessage += "REMOTE_ADDR = " + Request.ServerVariables["REMOTE_ADDR"] + Environment.NewLine;
        LogMessage += "HTTP_USER_AGENT = " + Request.ServerVariables["HTTP_USER_AGENT"] + Environment.NewLine;
        LogMessage += "CONTENT_TYPE = " + Request.ServerVariables["CONTENT_TYPE"] + Environment.NewLine;
        LogMessage += "SCRIPT_NAME = " + Request.ServerVariables["SCRIPT_NAME"] + Environment.NewLine;
        LogMessage += "REQUEST_METHOD = " + Request.ServerVariables["REQUEST_METHOD"] + Environment.NewLine;
        LogMessage += "Request.QueryString = " + Request.QueryString.ToString() + Environment.NewLine;
        LogMessage += "Request.Form = " + Request.Form.ToString() + Environment.NewLine;

        //Convert the Request.InputStream into a string.
        byte[] MyByteArray = new byte[Request.InputStream.Length];
        Request.InputStream.Read(MyByteArray, 0, Convert.ToInt32(Request.InputStream.Length));

        String InputStreamContents;
        InputStreamContents = System.Text.Encoding.UTF8.GetString(MyByteArray);

        LogMessage += "Request.InputStream = " + InputStreamContents + Environment.NewLine;
        LogMessage += Environment.NewLine;


        String TopicArn = "";
        
        if (InputStreamContents.StartsWith("{") == true)
        {
            //Convert the JSON data into a Dictionary.  Use of the System.Web.Script namespace requires a refrence to System.Web.Extensions.dll.
            System.Web.Script.Serialization.JavaScriptSerializer MyJavaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            System.Collections.Generic.Dictionary<String, Object> MyObjectDictionary;
            MyObjectDictionary = MyJavaScriptSerializer.DeserializeObject(InputStreamContents) as System.Collections.Generic.Dictionary<String, Object>;

            TopicArn = MyObjectDictionary["TopicArn"].ToString();
            
            if (MyObjectDictionary["Type"].ToString() == "SubscriptionConfirmation")
            {

                if (ValidateSignature == true)
                {
                    StringBuilder MyStringBuilder = new StringBuilder();
                    MyStringBuilder.Append("Message\n");
                    MyStringBuilder.Append(MyObjectDictionary["Message"].ToString()).Append("\n");
                    MyStringBuilder.Append("MessageId\n");
                    MyStringBuilder.Append(MyObjectDictionary["MessageId"].ToString()).Append("\n");
                    MyStringBuilder.Append("SubscribeURL\n");
                    MyStringBuilder.Append(MyObjectDictionary["SubscribeURL"].ToString()).Append("\n");
                    MyStringBuilder.Append("Timestamp\n");
                    MyStringBuilder.Append(MyObjectDictionary["Timestamp"].ToString()).Append("\n");
                    MyStringBuilder.Append("Token\n");
                    MyStringBuilder.Append(MyObjectDictionary["Token"].ToString()).Append("\n");
                    MyStringBuilder.Append("TopicArn\n");
                    MyStringBuilder.Append(MyObjectDictionary["TopicArn"].ToString()).Append("\n");
                    MyStringBuilder.Append("Type\n");
                    MyStringBuilder.Append(MyObjectDictionary["Type"].ToString()).Append("\n");

                    String GeneratedMessage;
                    GeneratedMessage = MyStringBuilder.ToString();

                    String SignatureFromAmazon;
                    SignatureFromAmazon = MyObjectDictionary["Signature"].ToString();

                    String SigningCertURL;
                    SigningCertURL = MyObjectDictionary["SigningCertURL"].ToString();

                    IsSignatureValid = VerifySignature(GeneratedMessage, SignatureFromAmazon, SigningCertURL);           
                    
                    LogMessage += "VerifyHash   IsSignatureValid = " + IsSignatureValid.ToString() + Environment.NewLine;
                    LogMessage += Environment.NewLine;
                }
                else
                {
                    IsSignatureValid = true;
                }


                if (IsSignatureValid == true)
                {
                    Boolean RetBool;
                    int ErrorNumber = 0;
                    String ErrorDescription = "";
                    String LogData = "";
                    Dictionary<String, String> RequestHeaders = new Dictionary<String, String>();
                    int ResponseStatusCode = 0;
                    String ResponseStatusDescription = "";
                    Dictionary<String, String> ResponseHeaders = new Dictionary<String, String>();
                    String ResponseString = "";
                                        
                    //This a SubscriptionConfirmation message.  Get the token.
                    String Token;
                    Token = MyObjectDictionary["Token"].ToString();

                    //Get the endpoint from the SigningCertURL value.
                    String TopicEndpoint;
                    TopicEndpoint = MyObjectDictionary["SigningCertURL"].ToString();
                    TopicEndpoint = TopicEndpoint.Substring(0, TopicEndpoint.IndexOf(".amazonaws.com/") + 15);
                    
                    //Amazon Simple Notification Service, ConfirmSubscription: http://docs.amazonwebservices.com/sns/latest/api/API_ConfirmSubscription.html
                    String RequestURL;
                    RequestURL = TopicEndpoint + "?Action=ConfirmSubscription&Version=2010-03-31";
                    RequestURL += "&Token=" + System.Uri.EscapeDataString(Token);
                    RequestURL += "&TopicArn=" + System.Uri.EscapeDataString(TopicArn);
                    
                    LogMessage += "Calling ConfirmSubscription on Amazon.   Token = " + Token + Environment.NewLine;
                    LogMessage += Environment.NewLine;

                    //Use the SprightlySoft AWS Component to call ConfirmSubscription on Amazon.
                    RetBool = MakeAmazonSignatureVersion2Request(AWSAccessKeyId, AWSSecretAccessKey, RequestURL, "GET", null, "", 3, ref ErrorNumber, ref ErrorDescription, ref LogData, ref RequestHeaders, ref ResponseStatusCode, ref ResponseStatusDescription, ref ResponseHeaders, ref ResponseString); ;

                    if (RetBool == true)
                    {
                        System.Xml.XmlDocument MyXmlDocument;
                        System.Xml.XmlNamespaceManager MyXmlNamespaceManager;
                        System.Xml.XmlNode MyXmlNode;

                        MyXmlDocument = new System.Xml.XmlDocument();
                        MyXmlDocument.LoadXml(ResponseString);

                        MyXmlNamespaceManager = new System.Xml.XmlNamespaceManager(MyXmlDocument.NameTable);
                        MyXmlNamespaceManager.AddNamespace("amz", "http://sns.amazonaws.com/doc/2010-03-31/");

                        MyXmlNode = MyXmlDocument.SelectSingleNode("amz:ConfirmSubscriptionResponse/amz:ConfirmSubscriptionResult/amz:SubscriptionArn", MyXmlNamespaceManager);

                        LogMessage += "ConfirmSubscription was successful.   SubscriptionArn = " + MyXmlNode.InnerText + Environment.NewLine;
                        LogMessage += Environment.NewLine;
                    }
                    else
                    {
                        LogMessage += "ConfirmSubscription failed." + Environment.NewLine;
                        LogMessage += "Response Status Code: " + ResponseStatusCode + Environment.NewLine;
                        LogMessage += "Response Status Description: " + ResponseStatusDescription + Environment.NewLine;
                        LogMessage += "Response String: " + ResponseString + Environment.NewLine;
                        LogMessage += Environment.NewLine;
                    }
                }
                else
                {
                    LogMessage += "The Signature is not valid.  No action was preformed." + Environment.NewLine;
                    LogMessage += Environment.NewLine;
                }
                
                
            }
            else if (MyObjectDictionary["Type"].ToString() == "Notification")
            {
                var userId = MyObjectDictionary["Message"].ToString();
                    
                if (ValidateSignature == true)
                {
                    
                    StringBuilder MyStringBuilder = new StringBuilder();
                    MyStringBuilder.Append("Message\n");
                    MyStringBuilder.Append(MyObjectDictionary["Message"].ToString()).Append("\n");
                    MyStringBuilder.Append("MessageId\n");
                    MyStringBuilder.Append(MyObjectDictionary["MessageId"].ToString()).Append("\n");

                    if (MyObjectDictionary.ContainsKey("Subject") == true)
                    {
                        MyStringBuilder.Append("Subject\n");
                        MyStringBuilder.Append(MyObjectDictionary["Subject"].ToString()).Append("\n");
                    }

                    MyStringBuilder.Append("Timestamp\n");
                    MyStringBuilder.Append(MyObjectDictionary["Timestamp"].ToString()).Append("\n");
                    MyStringBuilder.Append("TopicArn\n");
                    MyStringBuilder.Append(MyObjectDictionary["TopicArn"].ToString()).Append("\n");
                    MyStringBuilder.Append("Type\n");
                    MyStringBuilder.Append(MyObjectDictionary["Type"].ToString()).Append("\n");

                    String GeneratedMessage;
                    GeneratedMessage = MyStringBuilder.ToString();


                    String SignatureFromAmazon;
                    SignatureFromAmazon = MyObjectDictionary["Signature"].ToString();

                    String SigningCertURL;
                    SigningCertURL = MyObjectDictionary["SigningCertURL"].ToString();

                    IsSignatureValid = VerifySignature(GeneratedMessage, SignatureFromAmazon, SigningCertURL);

                    LogMessage += "VerifyHash   IsSignatureValid = " + IsSignatureValid.ToString() + Environment.NewLine;
                    LogMessage += Environment.NewLine;
                }
                else
                {
                    IsSignatureValid = true;
                }


                if (IsSignatureValid == true)
                {
                    SocialPayments.DataLayer.Interfaces.IDbContext ctx = new Context();
                    var userWorkFlow = new SocialPayments.Workflows.Users.UserWorkflow(ctx);

                    userWorkFlow.Execute(userId);

                }
                else
                {
                    LogMessage += "The Signature is not valid.  No action was preformed." + Environment.NewLine;
                    LogMessage += Environment.NewLine;
                }

            }
            else
            {
                LogMessage += "Nothing to do for this message type.   Type = " + MyObjectDictionary["Type"].ToString() + Environment.NewLine;
                LogMessage += Environment.NewLine;
            }
            
        }


        //Uncomment the following lines to save the log information to a text file.
        //System.IO.StreamWriter MyStreamWriter = new System.IO.StreamWriter("C:\\SNSAutoConfirm-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt", false, System.Text.Encoding.UTF8);
        //MyStreamWriter.Write(LogMessage);
        //MyStreamWriter.Close();
        
        
        //Send an email with the log information.
        System.Net.Mail.SmtpClient MySmtpClient = new System.Net.Mail.SmtpClient();
        MySmtpClient.Host = EmailSmtpHost;
        MySmtpClient.Port = EmailSmtpPort;
        MySmtpClient.EnableSsl = EmailSmtpEnableSsl;

        if (EmailSmtpUserName != "")
        {
            System.Net.NetworkCredential MyNetworkCredential = new System.Net.NetworkCredential();
            MyNetworkCredential.UserName = EmailSmtpUserName;
            MyNetworkCredential.Password = EmailSmtpPassword;
            MySmtpClient.Credentials = MyNetworkCredential;
        }

        System.Net.Mail.MailAddress FromMailAddress = new System.Net.Mail.MailAddress(EmailFromAddress);
        System.Net.Mail.MailAddress ToMailAddress = new System.Net.Mail.MailAddress(EmailToAddress);

        System.Net.Mail.MailMessage MyMailMessage = new System.Net.Mail.MailMessage(FromMailAddress, ToMailAddress);

        if (TopicArn == "")
        {
            MyMailMessage.Subject = "Log Information For " + Request.ServerVariables["SERVER_NAME"] + Request.ServerVariables["SCRIPT_NAME"];
        }
        else
        {
            MyMailMessage.Subject = "Log Information For " + TopicArn;
        }
        MyMailMessage.Body = LogMessage;

        MySmtpClient.Send(MyMailMessage);
        

        //Write the LogMessage to screen.
        LogMessage = Server.HtmlEncode(LogMessage);
        LogMessage = LogMessage.Replace(Environment.NewLine, "<br>");
        
        Response.Clear();
        Response.Write(LogMessage);
        Response.End();
    }


    private static Boolean VerifySignature(String GeneratedMessage, String SignatureFromAmazon, String SigningCertURL)
    {

        System.Uri MyUri = new System.Uri(SigningCertURL);

        //Check if the domain name in the SigningCertURL is an Amazon URL.
        if (MyUri.Host.EndsWith(".amazonaws.com") == true)
        {
            byte[] SignatureBytes;
            SignatureBytes = Convert.FromBase64String(SignatureFromAmazon);

            //Check the cache for the Amazon signing cert.
            byte[] PEMFileBytes;
            PEMFileBytes = (byte[])System.Web.HttpContext.Current.Cache[SigningCertURL];

            if (PEMFileBytes == null)
            {
                //Download the Amazon signing cert and save it to cache.
                System.Net.WebClient MyWebClient = new System.Net.WebClient();
                PEMFileBytes = MyWebClient.DownloadData(SigningCertURL);

                System.Web.HttpContext.Current.Cache[SigningCertURL] = PEMFileBytes;
            }

            System.Security.Cryptography.X509Certificates.X509Certificate2 MyX509Certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(PEMFileBytes);

            System.Security.Cryptography.RSACryptoServiceProvider MyRSACryptoServiceProvider;
            MyRSACryptoServiceProvider = (System.Security.Cryptography.RSACryptoServiceProvider)MyX509Certificate2.PublicKey.Key;

            System.Security.Cryptography.SHA1Managed MySHA1Managed = new System.Security.Cryptography.SHA1Managed();
            byte[] HashBytes = MySHA1Managed.ComputeHash(Encoding.UTF8.GetBytes(GeneratedMessage));

            return MyRSACryptoServiceProvider.VerifyHash(HashBytes, System.Security.Cryptography.CryptoConfig.MapNameToOID("SHA1"), SignatureBytes);            
        }
        else
        {
            return false;
        }
    }
    

    private static Boolean MakeAmazonSignatureVersion2Request(String AWSAccessKeyId, String AWSSecretAccessKey, String UserRequestURL, String RequestMethod, System.Collections.Generic.Dictionary<String, String> UserInputRequestHeaders, String SendData, int RetryTimes, ref int ErrorNumber, ref String ErrorDescription, ref String LogData, ref System.Collections.Generic.Dictionary<String, String> RequestHeaders, ref int ResponseStatusCode, ref String ResponseStatusDescription, ref System.Collections.Generic.Dictionary<String, String> ResponseHeaders, ref String ResponseStringFormatted)
    {
        String FullRequestURL;
        SprightlySoftAWS.REST MyREST = new SprightlySoftAWS.REST();
        Boolean RetBool = true;
        LogData = "";

        for (int i = 0; i <= RetryTimes; i++)
        {
            //Set the security and Signature parameters on the URL.
            FullRequestURL = UserRequestURL;
            FullRequestURL += "&AWSAccessKeyId=" + System.Uri.EscapeDataString(AWSAccessKeyId) + "&SignatureVersion=2&SignatureMethod=HmacSHA256&Timestamp=" + Uri.EscapeDataString(DateTime.UtcNow.ToString("yyyy-MM-dd\\THH:mm:ss.fff\\Z"));

            String SignatureValue;
            SignatureValue = MyREST.GetSignatureVersion2Value(FullRequestURL, RequestMethod, "", AWSSecretAccessKey);

            FullRequestURL += "&Signature=" + System.Uri.EscapeDataString(SignatureValue);

            //Send the request.
            RetBool = MyREST.MakeRequest(FullRequestURL, RequestMethod, UserInputRequestHeaders, SendData);

            //Set the return values.
            ErrorNumber = MyREST.ErrorNumber;
            ErrorDescription = MyREST.ErrorDescription;
            LogData += MyREST.LogData + Environment.NewLine;
            ResponseStatusCode = MyREST.ResponseStatusCode;
            ResponseStatusDescription = MyREST.ResponseStatusDescription;
            ResponseHeaders = MyREST.ResponseHeaders;
            ResponseStringFormatted = MyREST.ResponseStringFormatted;

            if (RetBool == true)
            {
                break;
            }
            else
            {
                if (MyREST.ResponseStatusCode == 500 || MyREST.ResponseStatusCode == 503)
                {
                    //A Service Unavailable response was returned.  Wait and retry.
                    System.Threading.Thread.Sleep(1000 * i * i);
                }
                else
                {
                    //An error occured but retrying would not solve the problem.
                    break;
                }
            }
        }

        return RetBool;
    }
    
    
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
