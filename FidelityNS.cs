using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veneka.Indigo.Integration;
using System.ComponentModel.Composition;
using Common.Logging;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.Fidelity.SMS;
using Veneka.Indigo.Integration.WebServices;
using Veneka.Indigo.Integration.Email;
using System.IO;
using Veneka.Indigo.Integration.Common;

namespace Veneka.Indigo.Integration.Fidelity
{
    //[IntegrationExport("FidelityNS", "D163EDB8-3302-4E37-8DA7-2E427AD22072", typeof(INotificationSystem))]
    //public class FidelityNS : INotificationSystem
    //{
    //    private static readonly ILog _nsLog = LogManager.GetLogger(General.Notification_LOGGER);

    //    public DirectoryInfo IntegrationFolder
    //    {
    //        get;set;
    //    }

    //    public string SQLConnectionString { get; set; }
    //    public IDataSource DataSource { get; set; }

    //    public bool Email(ref List<Notification> notifications, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
    //    {

    //        responseMessage = string.Empty;
    //        if (config is Config.WebServiceConfig)
    //        {
    //            if (!(config is Config.WebServiceConfig))
    //                throw new ArgumentException("WebService config parameters must be for SMSService.");

    //            var cbsParms = (Config.WebServiceConfig)config;


    //            string fromaddress = string.Empty, subject = string.Empty, messagetext = string.Empty, toaddress = string.Empty;


    //            WebServices.Protocol protocol = cbsParms.Protocol == Config.Protocol.HTTP ? WebServices.Protocol.HTTP : WebServices.Protocol.HTTPS;
    //            Authentication auth = cbsParms.AuthType == Config.AuthType.None ? Authentication.NONE : Authentication.BASIC;

    //            SMSWebService smsService = new SMSWebService(protocol, cbsParms.Address, cbsParms.Port, cbsParms.Path, cbsParms.Timeout, auth, cbsParms.Username, cbsParms.Password, cbsParms.Nonce, this.DataSource, true);
    //            if (notifications.Count() > 0)
    //            {
    //                List<string> _lst = new List<string>();
    //                foreach (var item in notifications.GroupBy(i => i.Address).Distinct())
    //                {
    //                    string _item = _lst.Find(i => i == item.Key);
    //                    if (string.IsNullOrEmpty(_item))
    //                    {
    //                        _lst.Add(item.Key);

    //                    }

    //                }
    //                toaddress = String.Join(";", _lst.ToArray());
    //                fromaddress = notifications[0].FromAddress;
    //                subject = notifications[0].Subject;
    //                messagetext = notifications[0].Text;
    //                //toaddress = toaddress.Remove(toaddress.Length - 1);
    //                _nsLog.Debug(string.Format("sending email: text: {0},subject: {1},fromaddress: {2},toaddress{3}", messagetext, subject, fromaddress, toaddress));

    //                return smsService.SendEmail(messagetext, subject, fromaddress, toaddress, out responseMessage);
    //            }
    //        }
    //        responseMessage = String.Empty;
    //        return false;
    //    }

    //    public bool SMS(ref List<Notification> notifications, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
    //    {
    //        responseMessage = string.Empty;
    //        if (config is Config.WebServiceConfig)
    //        {
    //            if (!(config is Config.WebServiceConfig))
    //                throw new ArgumentException("WebService config parameters must be for SMSService.");

    //            var cbsParms = (Config.WebServiceConfig)config;


    //            WebServices.Protocol protocol = cbsParms.Protocol == Config.Protocol.HTTP ? WebServices.Protocol.HTTP : WebServices.Protocol.HTTPS;
    //            Authentication auth = cbsParms.AuthType == Config.AuthType.None ? Authentication.NONE : Authentication.BASIC;

    //            SMSWebService smsService = new SMSWebService(protocol, cbsParms.Address, cbsParms.Port, cbsParms.Path, cbsParms.Timeout, auth, cbsParms.Username, cbsParms.Password, cbsParms.Nonce, this.DataSource, true);
    //            foreach (var item in notifications)
    //            {
    //                _nsLog.Debug("sending sms" + item.Address);
    //                item.IsSetn = smsService.SendSMS(item.Text, item.Address, out responseMessage);
    //            }
    //        }
    //        responseMessage = String.Empty;
    //        return true;
    //    }
    //}
}
