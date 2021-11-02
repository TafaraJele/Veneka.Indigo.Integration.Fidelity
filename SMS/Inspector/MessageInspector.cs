using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using Common.Logging;
using Veneka.Module.OracleFlexcube.Utils;

namespace Veneka.Indigo.Integration.Fidelity.SMS.Inspector
{
    public class MessageInspector : IClientMessageInspector
    {
        #region Private Fields
        private static ILog _log = LogManager.GetLogger("");
        private readonly bool _useBasicAuth;
        private static string _username { get; set; }
        private static string _password { get; set; }
        private static string _nonce { get; set; }

        #endregion

        #region Constructors
        public MessageInspector(bool useBasicAuth, string username, string password, string logger)
        {
            _useBasicAuth = useBasicAuth;
            _username = username;
            _password = password;
            _log = LogManager.GetLogger(logger);
        }
        public MessageInspector(bool useBasicAuth, string username, string password, string nonce, string logger)
        {
            _useBasicAuth = useBasicAuth;
            _username = username;
            _password = password;
            _nonce = nonce;
            _log = LogManager.GetLogger(logger);
        }

        #endregion

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Response:\t{0}", reply.ToString());
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Basic Authentication:\t", _useBasicAuth);

            //Basic auth mean we need to set username and password.
            if (_useBasicAuth && string.IsNullOrEmpty(_nonce))
            {
                StringBuilder header = new StringBuilder("basic ")
                                        .Append(Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + _password)));


                HttpRequestMessageProperty httpRequestMessage;
                object httpRequestMessageObject;
                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                {
                    httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                    if (string.IsNullOrEmpty(httpRequestMessage.Headers[HttpRequestHeader.Authorization]))
                    {
                        httpRequestMessage.Headers[HttpRequestHeader.Authorization] = header.ToString();
                    }
                }
                else
                {
                    var httpRequestProperty = new HttpRequestMessageProperty();
                    httpRequestProperty.Headers[HttpRequestHeader.Authorization] = header.ToString();
                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestProperty);
                }
            }
            else
            {
                // MessageHeaders messageHeadersElement = OperationContext.Current.OutgoingMessageHeaders;

                request.Headers.Add(new SecurityHeader("UsernameToken-1", _username, _password, _nonce));
            }


            int headerIndexOfAction = request.Headers.FindHeader("Action", "http://schemas.microsoft.com/ws/2005/05/addressing/none");
            if (headerIndexOfAction > -1)
                request.Headers.RemoveAt(headerIndexOfAction);


            if (_log.IsDebugEnabled)
                _log.DebugFormat("Request:\t{0}", request.ToString().Replace(_password, string.Empty.PadLeft(_password.Length, '*')));

            return null;
        }



        #endregion
    }
}
