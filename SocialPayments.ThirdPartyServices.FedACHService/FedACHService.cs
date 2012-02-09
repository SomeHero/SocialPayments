using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.ThirdPartyServices.FedACHService
{
    #region webservicex.net/FedACH.asmx


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "FedACHSoap", Namespace = "http://www.webservicex.net/")]
    public partial class FedACHService : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        private System.Threading.SendOrPostCallback getACHByNameOperationCompleted;

        private System.Threading.SendOrPostCallback getACHByLocationOperationCompleted;

        private System.Threading.SendOrPostCallback getACHByZipCodeOperationCompleted;

        private System.Threading.SendOrPostCallback getACHByFRBNumberOperationCompleted;

        private System.Threading.SendOrPostCallback getACHByRoutingNumberOperationCompleted;


        public FedACHService()
        {
            this.Url = "http://www.webservicex.net/FedACH.asmx";
        }


        public event getACHByNameCompletedEventHandler getACHByNameCompleted;


        public event getACHByLocationCompletedEventHandler getACHByLocationCompleted;


        public event getACHByZipCodeCompletedEventHandler getACHByZipCodeCompleted;

        /// <remarks/>
        public event getACHByFRBNumberCompletedEventHandler getACHByFRBNumberCompleted;


        public event getACHByRoutingNumberCompletedEventHandler getACHByRoutingNumberCompleted;


        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.webservicex.net/getACHByName", RequestNamespace = "http://www.webservicex.net/", ResponseNamespace = "http://www.webservicex.net/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool getACHByName(string Name, out FedACHList FedACHLists)
        {
            object[] results = this.Invoke("getACHByName", new object[] {
                    Name});
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public System.IAsyncResult BegingetACHByName(string Name, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("getACHByName", new object[] {
                    Name}, callback, asyncState);
        }


        public bool EndgetACHByName(System.IAsyncResult asyncResult, out FedACHList FedACHLists)
        {
            object[] results = this.EndInvoke(asyncResult);
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void getACHByNameAsync(string Name)
        {
            this.getACHByNameAsync(Name, null);
        }


        public void getACHByNameAsync(string Name, object userState)
        {
            if ((this.getACHByNameOperationCompleted == null))
            {
                this.getACHByNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetACHByNameOperationCompleted);
            }
            this.InvokeAsync("getACHByName", new object[] {
                    Name}, this.getACHByNameOperationCompleted, userState);
        }

        private void OngetACHByNameOperationCompleted(object arg)
        {
            if ((this.getACHByNameCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getACHByNameCompleted(this, new getACHByNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.webservicex.net/getACHByLocation", RequestNamespace = "http://www.webservicex.net/", ResponseNamespace = "http://www.webservicex.net/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool getACHByLocation(string Address, string StateCode, string City, out FedACHList FedACHLists)
        {
            object[] results = this.Invoke("getACHByLocation", new object[] {
                    Address,
                    StateCode,
                    City});
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public System.IAsyncResult BegingetACHByLocation(string Address, string StateCode, string City, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("getACHByLocation", new object[] {
                    Address,
                    StateCode,
                    City}, callback, asyncState);
        }

        /// <remarks/>
        public bool EndgetACHByLocation(System.IAsyncResult asyncResult, out FedACHList FedACHLists)
        {
            object[] results = this.EndInvoke(asyncResult);
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public void getACHByLocationAsync(string Address, string StateCode, string City)
        {
            this.getACHByLocationAsync(Address, StateCode, City, null);
        }


        public void getACHByLocationAsync(string Address, string StateCode, string City, object userState)
        {
            if ((this.getACHByLocationOperationCompleted == null))
            {
                this.getACHByLocationOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetACHByLocationOperationCompleted);
            }
            this.InvokeAsync("getACHByLocation", new object[] {
                    Address,
                    StateCode,
                    City}, this.getACHByLocationOperationCompleted, userState);
        }

        private void OngetACHByLocationOperationCompleted(object arg)
        {
            if ((this.getACHByLocationCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getACHByLocationCompleted(this, new getACHByLocationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }


        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.webservicex.net/getACHByZipCode", RequestNamespace = "http://www.webservicex.net/", ResponseNamespace = "http://www.webservicex.net/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool getACHByZipCode(string ZipCode, out FedACHList FedACHLists)
        {
            object[] results = this.Invoke("getACHByZipCode", new object[] {
                    ZipCode});
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public System.IAsyncResult BegingetACHByZipCode(string ZipCode, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("getACHByZipCode", new object[] {
                    ZipCode}, callback, asyncState);
        }


        public bool EndgetACHByZipCode(System.IAsyncResult asyncResult, out FedACHList FedACHLists)
        {
            object[] results = this.EndInvoke(asyncResult);
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public void getACHByZipCodeAsync(string ZipCode)
        {
            this.getACHByZipCodeAsync(ZipCode, null);
        }


        public void getACHByZipCodeAsync(string ZipCode, object userState)
        {
            if ((this.getACHByZipCodeOperationCompleted == null))
            {
                this.getACHByZipCodeOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetACHByZipCodeOperationCompleted);
            }
            this.InvokeAsync("getACHByZipCode", new object[] {
                    ZipCode}, this.getACHByZipCodeOperationCompleted, userState);
        }

        private void OngetACHByZipCodeOperationCompleted(object arg)
        {
            if ((this.getACHByZipCodeCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getACHByZipCodeCompleted(this, new getACHByZipCodeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.webservicex.net/getACHByFRBNumber", RequestNamespace = "http://www.webservicex.net/", ResponseNamespace = "http://www.webservicex.net/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool getACHByFRBNumber(string FRBNumber, out FedACHList FedACHLists)
        {
            object[] results = this.Invoke("getACHByFRBNumber", new object[] {
                    FRBNumber});
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public System.IAsyncResult BegingetACHByFRBNumber(string FRBNumber, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("getACHByFRBNumber", new object[] {
                    FRBNumber}, callback, asyncState);
        }


        public bool EndgetACHByFRBNumber(System.IAsyncResult asyncResult, out FedACHList FedACHLists)
        {
            object[] results = this.EndInvoke(asyncResult);
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public void getACHByFRBNumberAsync(string FRBNumber)
        {
            this.getACHByFRBNumberAsync(FRBNumber, null);
        }

        /// <remarks/>
        public void getACHByFRBNumberAsync(string FRBNumber, object userState)
        {
            if ((this.getACHByFRBNumberOperationCompleted == null))
            {
                this.getACHByFRBNumberOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetACHByFRBNumberOperationCompleted);
            }
            this.InvokeAsync("getACHByFRBNumber", new object[] {
                    FRBNumber}, this.getACHByFRBNumberOperationCompleted, userState);
        }

        private void OngetACHByFRBNumberOperationCompleted(object arg)
        {
            if ((this.getACHByFRBNumberCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getACHByFRBNumberCompleted(this, new getACHByFRBNumberCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.webservicex.net/getACHByRoutingNumber", RequestNamespace = "http://www.webservicex.net/", ResponseNamespace = "http://www.webservicex.net/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool getACHByRoutingNumber(string RoutingNumber, out FedACHList FedACHLists) //false if not a valid routing number
        {
            object[] results = this.Invoke("getACHByRoutingNumber", new object[] {
                    RoutingNumber});
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }


        public System.IAsyncResult BegingetACHByRoutingNumber(string RoutingNumber, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("getACHByRoutingNumber", new object[] {
                    RoutingNumber}, callback, asyncState);
        }


        public bool EndgetACHByRoutingNumber(System.IAsyncResult asyncResult, out FedACHList FedACHLists)
        {
            object[] results = this.EndInvoke(asyncResult);
            FedACHLists = ((FedACHList)(results[1]));
            return ((bool)(results[0]));
        }

        /// <remarks/>
        public void getACHByRoutingNumberAsync(string RoutingNumber)
        {
            this.getACHByRoutingNumberAsync(RoutingNumber, null);
        }

        /// <remarks/>
        public void getACHByRoutingNumberAsync(string RoutingNumber, object userState)
        {
            if ((this.getACHByRoutingNumberOperationCompleted == null))
            {
                this.getACHByRoutingNumberOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetACHByRoutingNumberOperationCompleted);
            }
            this.InvokeAsync("getACHByRoutingNumber", new object[] {
                    RoutingNumber}, this.getACHByRoutingNumberOperationCompleted, userState);
        }

        private void OngetACHByRoutingNumberOperationCompleted(object arg)
        {
            if ((this.getACHByRoutingNumberCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getACHByRoutingNumberCompleted(this, new getACHByRoutingNumberCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.webservicex.net/")]
    public partial class FedACHList
    {

        private FedACHData[] fedACHsField;

        private int totalRecordsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public FedACHData[] FedACHs
        {
            get
            {
                return this.fedACHsField;
            }
            set
            {
                this.fedACHsField = value;
            }
        }


        public int TotalRecords
        {
            get
            {
                return this.totalRecordsField;
            }
            set
            {
                this.totalRecordsField = value;
            }
        }
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.webservicex.net/")]
    public partial class FedACHData
    {

        private string routingNumberField;

        private string officeCodeField;

        private string servicingFRBNumberField;

        private string recordTypeCodeField;

        private string changeDateField;

        private string newRoutingNumberField;

        private string customerNameField;

        private string addressField;

        private string cityField;

        private string stateCodeField;

        private string zipcodeField;

        private string zipcodeExtensionField;

        private string telephoneAreaCodeField;

        private string telephonePrefixNumberField;

        private string telephoneSuffixNumberField;

        private string institutionStatusCodeField;


        public string RoutingNumber
        {
            get
            {
                return this.routingNumberField;
            }
            set
            {
                this.routingNumberField = value;
            }
        }


        public string OfficeCode
        {
            get
            {
                return this.officeCodeField;
            }
            set
            {
                this.officeCodeField = value;
            }
        }

        /// <remarks/>
        public string ServicingFRBNumber
        {
            get
            {
                return this.servicingFRBNumberField;
            }
            set
            {
                this.servicingFRBNumberField = value;
            }
        }


        public string RecordTypeCode
        {
            get
            {
                return this.recordTypeCodeField;
            }
            set
            {
                this.recordTypeCodeField = value;
            }
        }


        public string ChangeDate
        {
            get
            {
                return this.changeDateField;
            }
            set
            {
                this.changeDateField = value;
            }
        }


        public string NewRoutingNumber
        {
            get
            {
                return this.newRoutingNumberField;
            }
            set
            {
                this.newRoutingNumberField = value;
            }
        }


        public string CustomerName
        {
            get
            {
                return this.customerNameField;
            }
            set
            {
                this.customerNameField = value;
            }
        }


        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }


        public string City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }


        public string StateCode
        {
            get
            {
                return this.stateCodeField;
            }
            set
            {
                this.stateCodeField = value;
            }
        }

        /// <remarks/>
        public string Zipcode
        {
            get
            {
                return this.zipcodeField;
            }
            set
            {
                this.zipcodeField = value;
            }
        }


        public string ZipcodeExtension
        {
            get
            {
                return this.zipcodeExtensionField;
            }
            set
            {
                this.zipcodeExtensionField = value;
            }
        }


        public string TelephoneAreaCode
        {
            get
            {
                return this.telephoneAreaCodeField;
            }
            set
            {
                this.telephoneAreaCodeField = value;
            }
        }


        public string TelephonePrefixNumber
        {
            get
            {
                return this.telephonePrefixNumberField;
            }
            set
            {
                this.telephonePrefixNumberField = value;
            }
        }


        public string TelephoneSuffixNumber
        {
            get
            {
                return this.telephoneSuffixNumberField;
            }
            set
            {
                this.telephoneSuffixNumberField = value;
            }
        }


        public string InstitutionStatusCode
        {
            get
            {
                return this.institutionStatusCodeField;
            }
            set
            {
                this.institutionStatusCodeField = value;
            }
        }
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void getACHByNameCompletedEventHandler(object sender, getACHByNameCompletedEventArgs e);


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getACHByNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal getACHByNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
            :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public bool Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }


        public FedACHList FedACHLists
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((FedACHList)(this.results[1]));
            }
        }
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void getACHByLocationCompletedEventHandler(object sender, getACHByLocationCompletedEventArgs e);


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getACHByLocationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal getACHByLocationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
            :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }


        public bool Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }

        /// <remarks/>
        public FedACHList FedACHLists
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((FedACHList)(this.results[1]));
            }
        }
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void getACHByZipCodeCompletedEventHandler(object sender, getACHByZipCodeCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getACHByZipCodeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal getACHByZipCodeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
            :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }


        public bool Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }


        public FedACHList FedACHLists
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((FedACHList)(this.results[1]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void getACHByFRBNumberCompletedEventHandler(object sender, getACHByFRBNumberCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getACHByFRBNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal getACHByFRBNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
            :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public bool Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }

        /// <remarks/>
        public FedACHList FedACHLists
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((FedACHList)(this.results[1]));
            }
        }
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void getACHByRoutingNumberCompletedEventHandler(object sender, getACHByRoutingNumberCompletedEventArgs e);


    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getACHByRoutingNumberCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal getACHByRoutingNumberCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState)
            :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }


        public bool Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }


        public FedACHList FedACHLists
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((FedACHList)(this.results[1]));
            }
        }
    }
    #endregion
}
