using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.External;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.ProductPrinting;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityGOGFundsLoadCBS", "6986D248-6F8E-405C-B93B-B530C9839CE8", typeof(ICoreBankingSystem))]
    public class FidelityGoGFundsLoadCBS : ICoreBankingSystem
    {
        public IDataSource DataSource { get; set; }
        public DirectoryInfo IntegrationFolder { get; set; }
        private static readonly ILog _cbsLog = LogManager.GetLogger(General.CBS_LOGGER);

        public bool ChargeFee(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string feeRefrenceNumber, out string responseMessage)
        {
            feeRefrenceNumber = null;
            responseMessage = String.Empty;
            return true;
        }

        public bool CheckBalance(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;

        }

        public bool GetAccountDetail(string accountNumber, List<IProductPrintField> printFields, int cardIssueReasonId, int issuerId, int branchId, int productId, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out AccountDetails accountDetails, out string responseMessage)
        {
            responseMessage = string.Empty;
            accountDetails = new AccountDetails();

            if (config is Config.WebServiceConfig)
            {
                var cbsParms = (Config.WebServiceConfig)config;

                string protocol = cbsParms.Protocol.ToString();
                string Address = cbsParms.Address;
                string port = cbsParms.Port.ToString();
                string path = cbsParms.Path;


                _cbsLog.Debug("Before client");
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate (
            object sender,
            X509Certificate cert,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                    {
                        return true;   //Is valid
                    }
                    return true;

                };
                using (var client = new HttpClient())
                {
                    _cbsLog.Debug("calling get async");
                    _cbsLog.Debug("fileProcessingUrl 1==" + protocol + "://" + Address + ":" + port + "/" + path);
                    try
                    {
                        var content = client.GetAsync(protocol + "://" + Address + ":" + port + "/" + path + @"/api/accounts/" + accountNumber).Result;
                        // var content = client.GetAsync(fileProcessingUrl + @"/api/accounts/" + accountNumber).Result;
                        _cbsLog.Debug("fileProcessingUrl 2==" + protocol + "://" + Address + ":" + port + "/" + path + @"/api/accounts/" + accountNumber);
                        _cbsLog.Debug("content" + content);


                        if (content.IsSuccessStatusCode)
                        {
                            _cbsLog.Debug("status is success");
                            accountDetails = JsonConvert.DeserializeObject<AccountDetails>(content.Content.ReadAsStringAsync().Result);
                            accountDetails.CurrencyId = DecodeCurrency("GHS");
                            accountDetails.CBSAccountTypeId = "NotMapped";
                            accountDetails.ContactNumber = "+233000000000";
                            accountDetails.IsCBSAccount = true; //Fix for GoG : This is so that no teller notes denominations enabled for GoG customers

                            //var customers = client.GetAsync(fileProcessingUrl + @"/api/customers/customer/" + accountDetails.CustomerIDNumber).Result;
                            var customers = client.GetAsync(protocol + "://" + Address + ":" + port + "/" + path + @"/api/customers/customer/" + accountDetails.CustomerIDNumber).Result;
                            var customer = JsonConvert.DeserializeObject<Customer>(customers.Content.ReadAsStringAsync().Result);
                            accountDetails.ProductFields = new List<ProductField>();
                            _cbsLog.Debug("Calling productFields ");
                            foreach (var printField in printFields)
                            {
                                if (printField is PrintStringField)
                                {
                                    switch (printField.MappedName.ToLower())
                                    {
                                        case "ind_sys_dob":
                                            ((PrintStringField)printField).Value = customer.DateOfBirth;
                                            accountDetails.ProductFields.Add(new ProductField(printField));
                                            _cbsLog.Debug("Date of birth" + customer.DateOfBirth);
                                            break;
                                        case "ind_sys_address":
                                            ((PrintStringField)printField).Value = "";
                                            accountDetails.ProductFields.Add(new ProductField(printField));
                                            //_cbsLog.Debug("address" + response.adesc);
                                            break;
                                        default: accountDetails.ProductFields.Add(new ProductField(printField)); break;
                                    }

                                    //accountDetails.ProductFields.Add(new ProductField(item));

                                }
                            }
                        }

                        else
                        {
                            _cbsLog.Debug("failed");
                            responseMessage = "failed";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _cbsLog.Debug("exception==" + ex.ToString());
                        _cbsLog.Debug("innerexception==" + ex.InnerException);
                        _cbsLog.Debug("stacktrace==" + ex.StackTrace);

                        throw new ArgumentException("Exception ", ex);
                    }
                }
            }
            responseMessage = "success";
            return true;
        }

        public bool ReverseFee(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        public bool UpdateAccount(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        private int DecodeCurrency(string ccy)
        {
            return DataSource.LookupDAL.LookupCurrency(ccy);

        }
    }
      
    }

