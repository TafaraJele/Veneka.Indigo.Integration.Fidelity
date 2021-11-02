using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using Common.Logging;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.Fidelity.FBLNotificationService;
using Veneka.Indigo.Integration.WebServices;

namespace Veneka.Indigo.Integration.Fidelity.SMS
{
    //class SMSWebService : WebServices.WebService
    //{
    //    #region Private Fields
    //    private static readonly ILog _notificationsLog = LogManager.GetLogger(General.Notification_LOGGER);

    //    private readonly NotificationPortTypeClient _client;


    //    #endregion

    //    #region Constructors


    //    public SMSWebService(WebServices.Protocol protocol, string address, int port, string path, int? timeoutMilliSeconds, Authentication authentication, string username, string password, string nonce, IDataSource DataSource, bool ignoreUntrustedSSL)
    //        : base(protocol, address, port, path, timeoutMilliSeconds, authentication, username, password,DataSource, General.CPS_LOGGER)
    //    {
    //        UriBuilder uri = new UriBuilder
    //        {
    //            Scheme = protocol.ToString(),
    //            Host = address,
    //            Port = port,
    //            Path = path
    //        };

    //        //Register the trace extention before creating the proxy
    //        // RegisterSoapExtension(typeof(FlexTraceExtension), 1, PriorityGroup.Low);

    //        BasicHttpBinding binding = BuildBindings("SMSService", protocol, timeoutMilliSeconds);
    //        EndpointAddress endpointaddress = BuildEndpointAddress(protocol, address, port, path);

    //        _client = new NotificationPortTypeClient(binding, endpointaddress);


    //        _client.Endpoint.Behaviors.Add(new Inspector.LogClientBehaviour(false, username, password, nonce, General.CPS_LOGGER));



    //        _log.Info("URL: " + uri.Uri.ToString());

    //        IgnoreUntrustedSSL = ignoreUntrustedSSL;

    //        _log.Trace(t => t("Done setting up client."));

    //    }
    //    public bool SendSMS(string message, string number, out string responsemessage)
    //    {
    //        try
    //        {
    //            AddUntrustedSSL();
    //            SendSms_Req request = new SendSms_Req();
    //            request.Message = message;
    //            request.RecipientPhoneNumber = number;
    //            request.SenderChannel = "ICI";

    //            SendSms_Resp response = _client.SendSms(request);
    //            responsemessage = response.message.ToString();

    //            return true;
    //        }
    //        catch (FaultException ex)
    //        {
    //            responsemessage = ex.Message;
    //            this._log.Debug(ex);
    //            return false;
    //        }
    //    }
    //    protected new void AddUntrustedSSL()
    //    {
    //        if (_log.IsDebugEnabled)
    //            _log.DebugFormat("Ignore Untrusted SSL:\t", IgnoreUntrustedSSL);

    //        if (IgnoreUntrustedSSL)

    //        {
    //            //                ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
    //            //(SecurityProtocolType)768 | (SecurityProtocolType)3072 | (SecurityProtocolType)48;

    //            ServicePointManager
    //.ServerCertificateValidationCallback +=
    //(sender, cert, chain, sslPolicyErrors) => true;
    //            //ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate,
    //            //                                                            X509Chain chain,
    //            //                                                            SslPolicyErrors sslPolicyErrors) => true;
    //        }
    //    }
    //    public bool SendEmail(string message, string subject, string fromaddress, string toaddress, out string responsemessage)
    //    {
    //        try
    //        {
    //            AddUntrustedSSL();
    //            SendEmail_Req request = new SendEmail_Req();
    //            request.Message = message;
    //            request.Subject = subject;
    //            request.Recipient = toaddress;
    //            request.CC = string.Empty;
    //            request.BCC = toaddress;
    //            request.SenderChannel = "ICI";
    //            request.FromEmail = fromaddress;

    //            SendEmail_Resp response = _client.SendEmail(request);

    //            responsemessage = response.message.ToString();

    //            return true;
    //        }
    //        catch (FaultException ex)
    //        {
    //            responsemessage = ex.Message;
    //            this._log.Debug(ex);
    //            return false;
    //        }
    //    }

    //    #endregion
    //}
}
