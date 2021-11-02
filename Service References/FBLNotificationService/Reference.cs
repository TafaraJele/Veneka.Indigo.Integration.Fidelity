﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Veneka.Indigo.Integration.Fidelity.FBLNotificationService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://new.webservice.namespace", ConfigurationName="FBLNotificationService.NotificationPortType")]
    public interface NotificationPortType {
        
        // CODEGEN: Generating message contract since the operation SendSms is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action="SendSms", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_output SendSms(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_input request);
        
        // CODEGEN: Generating message contract since the operation SendEmail is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action="SendEmail", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_output SendEmail(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_input request);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1064.2")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://new.webservice.namespace")]
    public partial class SendSms_Req : object, System.ComponentModel.INotifyPropertyChanged {
        
        private object senderChannelField;
        
        private string messageField;
        
        private string recipientPhoneNumberField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public object SenderChannel {
            get {
                return this.senderChannelField;
            }
            set {
                this.senderChannelField = value;
                this.RaisePropertyChanged("SenderChannel");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
                this.RaisePropertyChanged("Message");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string RecipientPhoneNumber {
            get {
                return this.recipientPhoneNumberField;
            }
            set {
                this.recipientPhoneNumberField = value;
                this.RaisePropertyChanged("RecipientPhoneNumber");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1064.2")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://new.webservice.namespace")]
    public partial class SendSms_Resp : object, System.ComponentModel.INotifyPropertyChanged {
        
        private object messageField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public object message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
                this.RaisePropertyChanged("message");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SendSms_input {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://new.webservice.namespace", Order=0)]
        public Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_Req SendSms_Req;
        
        public SendSms_input() {
        }
        
        public SendSms_input(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_Req SendSms_Req) {
            this.SendSms_Req = SendSms_Req;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SendSms_output {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://new.webservice.namespace", Order=0)]
        public Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_Resp SendSms_Resp;
        
        public SendSms_output() {
        }
        
        public SendSms_output(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_Resp SendSms_Resp) {
            this.SendSms_Resp = SendSms_Resp;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1064.2")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://new.webservice.namespace")]
    public partial class SendEmail_Req : object, System.ComponentModel.INotifyPropertyChanged {
        
        private object fromEmailField;
        
        private object senderChannelField;
        
        private object subjectField;
        
        private object recipientField;
        
        private object ccField;
        
        private object bCCField;
        
        private object messageField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public object FromEmail {
            get {
                return this.fromEmailField;
            }
            set {
                this.fromEmailField = value;
                this.RaisePropertyChanged("FromEmail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public object SenderChannel {
            get {
                return this.senderChannelField;
            }
            set {
                this.senderChannelField = value;
                this.RaisePropertyChanged("SenderChannel");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public object Subject {
            get {
                return this.subjectField;
            }
            set {
                this.subjectField = value;
                this.RaisePropertyChanged("Subject");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public object Recipient {
            get {
                return this.recipientField;
            }
            set {
                this.recipientField = value;
                this.RaisePropertyChanged("Recipient");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public object CC {
            get {
                return this.ccField;
            }
            set {
                this.ccField = value;
                this.RaisePropertyChanged("CC");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public object BCC {
            get {
                return this.bCCField;
            }
            set {
                this.bCCField = value;
                this.RaisePropertyChanged("BCC");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=6)]
        public object Message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
                this.RaisePropertyChanged("Message");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1064.2")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://new.webservice.namespace")]
    public partial class SendEmail_Resp : object, System.ComponentModel.INotifyPropertyChanged {
        
        private object messageField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public object message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
                this.RaisePropertyChanged("message");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SendEmail_input {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://new.webservice.namespace", Order=0)]
        public Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_Req SendEmail_Req;
        
        public SendEmail_input() {
        }
        
        public SendEmail_input(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_Req SendEmail_Req) {
            this.SendEmail_Req = SendEmail_Req;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SendEmail_output {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://new.webservice.namespace", Order=0)]
        public Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_Resp SendEmail_Resp;
        
        public SendEmail_output() {
        }
        
        public SendEmail_output(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_Resp SendEmail_Resp) {
            this.SendEmail_Resp = SendEmail_Resp;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface NotificationPortTypeChannel : Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class NotificationPortTypeClient : System.ServiceModel.ClientBase<Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType>, Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType {
        
        public NotificationPortTypeClient() {
        }
        
        public NotificationPortTypeClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public NotificationPortTypeClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public NotificationPortTypeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public NotificationPortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_output Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType.SendSms(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_input request) {
            return base.Channel.SendSms(request);
        }
        
        public Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_Resp SendSms(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_Req SendSms_Req) {
            Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_input inValue = new Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_input();
            inValue.SendSms_Req = SendSms_Req;
            Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendSms_output retVal = ((Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType)(this)).SendSms(inValue);
            return retVal.SendSms_Resp;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_output Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType.SendEmail(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_input request) {
            return base.Channel.SendEmail(request);
        }
        
        public Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_Resp SendEmail(Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_Req SendEmail_Req) {
            Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_input inValue = new Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_input();
            inValue.SendEmail_Req = SendEmail_Req;
            Veneka.Indigo.Integration.Fidelity.FBLNotificationService.SendEmail_output retVal = ((Veneka.Indigo.Integration.Fidelity.FBLNotificationService.NotificationPortType)(this)).SendEmail(inValue);
            return retVal.SendEmail_Resp;
        }
    }
}