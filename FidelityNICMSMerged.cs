using Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.EMPDebitMerged;
using Veneka.Indigo.Integration.EMPDebitMerged.FIMI;
using Veneka.Indigo.Integration.External;
using Veneka.Indigo.Integration.Fidelity.BankWorld;
using Veneka.Indigo.Integration.Fidelity.BranchFile;
using Veneka.Indigo.Integration.Fidelity.BranchFile.Objects;
using Veneka.Indigo.Integration.Fidelity.DAL;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.Remote;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityNICMSMerged", "0387B456-22FA-4DCB-AD7F-AF280BF3DFA8", typeof(ICardManagementSystem))]
    public class FidelityNICMSMerged : ICardManagementSystem
    {
        private static readonly ILog _cmsLog = LogManager.GetLogger(General.CMS_LOGGER);
        public string SQLConnectionString { get; set; }
        public bool OnUploadCompletedSubscribed { get ; set ; }
        public IDataSource DataSource { get ; set ; }
        public DirectoryInfo IntegrationFolder { get ; set ; }

        public event EventHandler<DistEventArgs> OnUploadCompleted;

        public bool AccountLookup(int issuerId, int productId, int cardIssueReasonId, string accountNumber, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, ref AccountDetails accountDetails, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        public LinkResponse ActiveCard(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            responseMessage = String.Empty;

            try
            {
                WebServiceConfig webConfig = null;
                if (config is WebServiceConfig)
                    webConfig = (WebServiceConfig)config;
                else
                    throw new ArgumentException("Config parameters must be for Webservice.");

                //split address field for bankworld and NI cms ;
                string fullAddress = webConfig.Address;
                _cmsLog.Debug("fullAddress =" + fullAddress);
                string[] details = fullAddress.Split(';');
                _cmsLog.Debug("details =" + details);
                string bankworldAddress = string.Empty;
                string niAddress = string.Empty;
                
                if (details == null)
                {
                    responseMessage = "Address field is not conigured for Bankworld or NI";
                }
                else
                {
                    if (details.Length == 2)
                    {
                        bankworldAddress = details[0];
                        niAddress = details[1];
                       
                        _cmsLog.Debug("bankworldAddress =" + bankworldAddress);
                        _cmsLog.Debug("niAddress =" + niAddress);
                       
                    }
                    else
                    {
                        responseMessage = "bankworld Address;NIAddress are not configured correctly for CMS";
                        throw new ArgumentException("bankworld Address;NIAddress are not configured correctly for CMS");
                    }
                }
                string niUsername = webConfig.RemoteUsername;
                string niPassword = webConfig.RemotePassword;
                int niPort = Convert.ToInt16(webConfig.RemotePort);


                FIMIWebService fimiService = new FIMIWebService(niAddress, "", niPort, niUsername, niPassword, new DefaultDataDAL(this.DataSource));
                BankWorldWebService bankWorldService = new BankWorldWebService(bankworldAddress, webConfig.Path, webConfig.Port, webConfig.Username, webConfig.Password, DataSource);

                string fullname = customerDetails.FirstName;

                if (!String.IsNullOrWhiteSpace(customerDetails.MiddleName))
                    fullname += " " + customerDetails.MiddleName;

                if (!String.IsNullOrWhiteSpace(customerDetails.LastName))
                    fullname += " " + customerDetails.LastName;

                string branchCode = DataSource.LookupDAL.LookupEmpBranchCode(customerDetails.BranchId);

                //string branchCode = branchCodes[customerDetails.BranchId];

                if (String.IsNullOrWhiteSpace(branchCode))
                    throw new ArgumentNullException("EMPBranchCode", "Branch does not have an EMP branch code specified.");


                DateTime dob = DateTime.Now;
                string address = String.Empty;
                string postCode = String.Empty;

                if (customerDetails.ProductFields != null && customerDetails.ProductFields.Count > 0)
                {
                    foreach (var field in customerDetails.ProductFields)
                    {
                        if (field.MappedName.Equals("ind_sys_dob", StringComparison.OrdinalIgnoreCase))
                        {
                            string dateofBirth = Encoding.ASCII.GetString(field.Value);
                            dob = DateTime.ParseExact(dateofBirth, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                            
                            _cmsLog.Debug("EMPDOB field" + dob);
                        }

                        if (field.MappedName.Equals("ind_sys_address", StringComparison.OrdinalIgnoreCase))
                        {
                            address = Encoding.UTF8.GetString(field.Value);
                            _cmsLog.Debug("EMP ADDRESS " + address);
                        }

                        if (field.MappedName.Equals("ind_sys_postcode", StringComparison.OrdinalIgnoreCase))
                        {
                            postCode = Encoding.UTF8.GetString(field.Value);
                            _cmsLog.Debug("EMP Postal Code" + postCode);
                        }
                    }
                }

                // Comment field is built up by emp product account type and currency, and branch code
                // Each product's currency must have the emp_account_type variable set.

                string empAccountType = String.Empty;
                if (!customerDetails.CurrencyFields.TryGetValue("emp_account_type", out empAccountType))
                {
                    responseMessage = String.Format("emp_account_type not set for product currency {0}. Please check product currency configuration.", customerDetails.CurrencyCode);
                    return LinkResponse.RETRY;
                }

                string comment = String.Format("{0}~{1}", empAccountType, branchCode);

               
                int cms_account_type;
                if (!string.IsNullOrEmpty(customerDetails.CMSAccountType))
                {
                    int.TryParse(customerDetails.CMSAccountType, out cms_account_type);
                }
                else
                {
                    throw new Exception("CMSAccountType is null");
                }
                var item = externalFields.Field.FirstOrDefault(i => i.Key == "cardType");
                if (item.Key == null)
                {
                    throw new Exception("cardType external field not found.");
                }
                item = externalFields.Field.FirstOrDefault(i => i.Key == "cashLimit");
                if (item.Key == null)
                {
                    throw new Exception("cashLimit external field not found.");
                }
                item = externalFields.Field.FirstOrDefault(i => i.Key == "serviceCode");
                if (item.Key == null)
                {
                    throw new Exception("serviceCode external field not found.");
                }
                string cardType = externalFields.Field["cardType"].ToString();
                string cashLimit = externalFields.Field["cashLimit"].ToString();
                string serviceCode = externalFields.Field["serviceCode"].ToString();
                CardDetail card = new CardDetail();
                ////added below to get the pan to update bankworld for Fidelity
                if (fimiService.FetchPan(int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, customerDetails.CustomerIDNumber, out string PAN, out DateTime? expiryDate, out responseMessage))
                    customerDetails.CardNumber = PAN;
                card.card_expiry_date = expiryDate;
                _cmsLog.Debug("PAN==" + customerDetails.CardNumber);
                _cmsLog.Debug("expiryDat==e" + expiryDate);
                //// Added till here for Fidelity to update bankworld

                if (fimiService.LinkCardToAccountAndActive(customerDetails.CardId, int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, fullname, customerDetails.AccountNumber,
               cms_account_type, int.Parse(DataSource.LookupDAL.LookupCurrencyISONumericCode(customerDetails.CurrencyId.Value)), customerDetails.CustomerIDNumber, dob,
               address, postCode, comment, auditUserId, auditWorkStation, out responseMessage, out PAN))

                    if (bankWorldService.CreateCard(customerDetails, card.card_expiry_date, cardType, cashLimit, address, serviceCode, languageId, out responseMessage))

                        return LinkResponse.SUCCESS;
                    else
                        return LinkResponse.ERROR;
            }



            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cmsLog.Error(endpointException);
                responseMessage = "Unable to connect to FIMI, please try again or contact support.";
                return LinkResponse.RETRY;
            }
            catch (Exception ex)
            {
                _cmsLog.Error(ex);
                responseMessage = ex.Message;
            }

            return LinkResponse.ERROR;
        }

        public LinkResponse LinkCardsToAccount(List<CustomerDetails> customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out Dictionary<long, LinkResponse> response, out string responseMessage)
        {
            throw new NotImplementedException();
        }

        public LinkResponse LinkCardToAccountAndActive(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            _cmsLog.Debug("Fidelity NI CMS Merged class Link card to account and activate");
            responseMessage = String.Empty;

            try
            {
                _cmsLog.Debug("Pick the webservice configs");
                WebServiceConfig webConfig = null;
                if (config is WebServiceConfig)
                    webConfig = (WebServiceConfig)config;
                else
                    throw new ArgumentException("Config parameters must be for Webservice.");

                //split address field for bankworld and NI cms ;
                string fullAddress = webConfig.Address;
                _cmsLog.Debug("fullAddress =" + fullAddress);
                string[] details = fullAddress.Split(';');
                _cmsLog.Debug("details =" + details);
                string bankworldAddress = string.Empty;
                string niAddress = string.Empty;
                //string niPort = string.Empty;
                if (details == null)
                {
                    responseMessage = "Address field is not conigured for Bankworld or NI";
                }
                else
                {
                    if (details.Length == 2)
                    {
                        bankworldAddress = details[0];
                        niAddress = details[1];
                       
                        _cmsLog.Debug("bankworldAddress =" + bankworldAddress);
                        _cmsLog.Debug("niAddress =" + niAddress);
                        
                    }
                    else
                    {
                        responseMessage = "bankworld Address;NIAddress are not configured correctly for CMS";
                        throw new ArgumentException("bankworld Address;NIAddress are not configured correctly for CMS");
                    }
                }

                string niUsername = webConfig.RemoteUsername;
                string niPassword = webConfig.RemotePassword;
                int niPort = Convert.ToInt16(webConfig.RemotePort);


                FIMIWebService fimiService = new FIMIWebService(niAddress, "", niPort, niUsername, niPassword, new DefaultDataDAL(this.DataSource));
                BankWorldWebService bankWorldService = new BankWorldWebService(bankworldAddress, webConfig.Path, webConfig.Port, webConfig.Username, webConfig.Password, DataSource);

                string fullname = customerDetails.FirstName;

                if (!String.IsNullOrWhiteSpace(customerDetails.MiddleName))
                    fullname += " " + customerDetails.MiddleName;

                if (!String.IsNullOrWhiteSpace(customerDetails.LastName))
                    fullname += " " + customerDetails.LastName;

                string branchCode = DataSource.LookupDAL.LookupEmpBranchCode(customerDetails.BranchId);

                //string branchCode = branchCodes[customerDetails.BranchId];

                if (String.IsNullOrWhiteSpace(branchCode))
                    throw new ArgumentNullException("EMPBranchCode", "Branch does not have an EMP branch code specified.");


                DateTime dob = DateTime.Now;
                string address = String.Empty;
                string postCode = String.Empty;               

                if (customerDetails.ProductFields != null && customerDetails.ProductFields.Count > 0)
                {
                    foreach (var field in customerDetails.ProductFields)
                    {
                        if (field.MappedName.Equals("ind_sys_dob", StringComparison.OrdinalIgnoreCase))
                        {
                            //string dateofBirth = Encoding.ASCII.GetString(field.Value);
                            //dob = DateTime.ParseExact(dateofBirth, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                           // dob = DateTime.Parse(Encoding.UTF8.GetString(field.Value), CultureInfo.InvariantCulture);
                            string dateofBirth = Encoding.ASCII.GetString(field.Value);
                            DateTime.TryParse(dateofBirth, out dob);
                            _cmsLog.Debug("EMPDOB field" + dob);
                        }

                        if (field.MappedName.Equals("ind_sys_address", StringComparison.OrdinalIgnoreCase))
                        {
                            address = Encoding.UTF8.GetString(field.Value);
                            _cmsLog.Debug("EMP ADDRESS " + address);
                        }

                        if (field.MappedName.Equals("ind_sys_postcode", StringComparison.OrdinalIgnoreCase))
                        {
                            postCode = Encoding.UTF8.GetString(field.Value);
                            _cmsLog.Debug("EMP Postal Code" + postCode);
                        }
                    }
                }

                // Comment field is built up by emp product account type and currency, and branch code
                // Each product's currency must have the emp_account_type variable set.

                string empAccountType = String.Empty;
                if (!customerDetails.CurrencyFields.TryGetValue("emp_account_type", out empAccountType))
                {
                    responseMessage = String.Format("emp_account_type not set for product currency {0}. Please check product currency configuration.", customerDetails.CurrencyCode);
                    return LinkResponse.RETRY;
                }

                string comment = String.Format("{0}~{1}", empAccountType, branchCode);

                
                int cms_account_type;
                if (!string.IsNullOrEmpty(customerDetails.CMSAccountType))
                {
                    int.TryParse(customerDetails.CMSAccountType, out cms_account_type);
                }
                else
                {
                    throw new Exception("CMSAccountType is null");
                }

                var item = externalFields.Field.FirstOrDefault(i => i.Key == "cardType");
                if (item.Key == null)
                {
                    throw new Exception("cardType external field not found.");
                }
                item = externalFields.Field.FirstOrDefault(i => i.Key == "cashLimit");
                if (item.Key == null)
                {
                    throw new Exception("cashLimit external field not found.");
                }
                item = externalFields.Field.FirstOrDefault(i => i.Key == "serviceCode");
                if (item.Key == null)
                {
                    throw new Exception("serviceCode external field not found.");
                }

                CardDetail card = new CardDetail();

                string cardType = externalFields.Field["cardType"].ToString();
                string cashLimit = externalFields.Field["cashLimit"].ToString();
                string serviceCode = externalFields.Field["serviceCode"].ToString();
                //added below to get the pan to update bankworld for Fidelity
                if (fimiService.FetchPan(int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, customerDetails.CustomerIDNumber, out string PAN, out DateTime? expiryDate, out responseMessage))
                    customerDetails.CardNumber = PAN;
                _cmsLog.Debug("Log 1 PAN =" + PAN + "for customer=" + customerDetails.CardReference);
                card.card_expiry_date = expiryDate;
                _cmsLog.Debug("Log 1 expiryDate " + expiryDate);
                // Added till here for Fidelity to update bankworld

                if (fimiService.LinkCardToAccountAndActive(customerDetails.CardId, int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, fullname, customerDetails.AccountNumber,
               cms_account_type, int.Parse(DataSource.LookupDAL.LookupCurrencyISONumericCode(customerDetails.CurrencyId.Value)), customerDetails.CustomerIDNumber, dob,
               address, postCode, comment, auditUserId, auditWorkStation, out responseMessage, out PAN))
                   
                    _cmsLog.Debug($"Update card number for card id {customerDetails.CardId}");
                DataSource.CardsDAL.UpdateCardNumber(customerDetails.CardId, PAN, auditUserId, auditWorkStation);


                _cmsLog.Debug("PAN =" + PAN + "for customer=" + customerDetails.CardReference);
                if (bankWorldService.CreateCard(customerDetails, card.card_expiry_date, cardType, cashLimit, address, serviceCode, languageId, out responseMessage))

                    return LinkResponse.SUCCESS;
                else
                    return LinkResponse.ERROR;
            }



            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cmsLog.Error(endpointException);
                responseMessage = "Unable to connect to FIMI, please try again or contact support.";
                return LinkResponse.RETRY;
            }
            catch (Exception ex)
            {
                _cmsLog.Error(ex);
                responseMessage = ex.Message;
            }

            return LinkResponse.ERROR;
        }

        public bool RemoteFetchDetails(List<CardDetail> cardDetails, ExternalSystemFields externalFields, IConfig config, out List<CardDetailResponse> failedCards, out string responseMessage)
        {
            throw new NotImplementedException();
        }

        public bool SpoilCard(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            switch (languageId)
            {
                case 0: responseMessage = "Action was successful."; break;
                case 1: responseMessage = "Action reussie."; break;
                case 2: responseMessage = "Action was successful_pt"; break;
                case 3: responseMessage = "Action was successful_sp"; break;
                default:
                    responseMessage = "Action was successful.";
                    break;
            }

            return true;
        }

        public bool UpdatePVV(int issuerId, int productId, Track2 track2, string PVV, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            try
            {
                responseMessage = String.Empty;

                //ParametersDAL pDal = new ParametersDAL(this.SQLConnectionString);
                //LookupDAL lDal = new LookupDAL(this.SQLConnectionString);

                //First get parameter for CMS web service call
                //var cmsParms = pDal.GetParameter(issuerId, 1, 1, null, auditUserId, auditWorkStation);

                //if (cmsParms == null)
                //    throw new ArgumentNullException("Cannot find parameters for Card Management System.");

                if (!(config is WebServiceConfig))
                    throw new ArgumentException("CMS config parameters must be for Webservice.");

                BankWorldWebService bankWorldService = new BankWorldWebService((WebServiceConfig)config, DataSource);

                WriteToLog("UpdatePVV: PAN=" + track2.PAN + ", PVVOffset=" + PVV);

                if (!bankWorldService.UpdatePVV(track2.PAN, PVV, languageId, out responseMessage))
                    return false;

                responseMessage = "Success";
                return true;
            }
            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cmsLog.Error(endpointException);
                responseMessage = "Unable to connect to BankWorld, please try again or contact support.";
            }
            catch (Exception ex)
            {
                _cmsLog.Error(ex);
                responseMessage = ex.Message;
            }

            return false;
        }

        public bool UploadGeneratedCards(List<CardObject> cardObjects, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            ///-----------------------MIGRATION BELOW-------------------------------------------
            _cmsLog.Debug("calling upload method in Fidelity CMS");
            EMPDEBITCMS empDebitCMS = new EMPDEBITCMS(this.DataSource);
            responseMessage = String.Empty;
            _cmsLog.Debug("calling upload method in Fidelity CMS step 2");
            empDebitCMS.UploadGeneratedCards(cardObjects, externalFields, config, languageId, auditUserId, auditWorkStation, out string rspMessages);
            _cmsLog.Debug("calling upload method in Fidelity CMS step 3");
            responseMessage = rspMessages;
            return true;
            ///-------------------------------ABOVE MIGRATION-----------------------------
        }

        public bool ValidateCustomerDetails(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        public void GenerateBranchFile(List<CardObject> cardObjects, string outputDirectory, string fileName)
        {
            if (!outputDirectory.EndsWith("\\"))
                outputDirectory += "\\";

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = "BranchFile_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
            }

            if (!fileName.EndsWith(".txt"))
                fileName += ".txt";            

            int count = 0;
            long sequence = DataSource.TransactionSequenceDAL.NextSequenceNumber("BFSequenceHeader", 0);

            //for CardReresh file
            string issuercode = string.Empty;
            string productcode = string.Empty;
            string productionBatchrefrence = string.Empty;
            if (cardObjects.Count > 0)
            {
                issuercode = cardObjects.First().IssuerCode;
                productcode = cardObjects.First().ProductCode;
                productionBatchrefrence = cardObjects.First().DistBatchReference;
            }

            HeaderRecord header = new HeaderRecord(issuercode, productcode, DateTime.Now, sequence.ToString(), productionBatchrefrence);
            List<DataRecord> dtlist = new List<DataRecord>();
            DataRecord dr;


            foreach (var card in cardObjects)
            {
                dr = new DataRecord(card.BIN, card.CardNumber, card.CardReferenceNumber, card.DeliveryBranchCode + "_" + card.DistBatchReference, card.DeliveryBranchName, card.DeliveryBranchCode);
                dtlist.Add(dr);
            }

            TrailerRecord trailerecord = new TrailerRecord("EOF", dtlist.Count());
            FileGenerator fg = new FileGenerator();
            fg.CreateBranchFile(fileName, header, dtlist, trailerecord, outputDirectory);
          
        }

        [Conditional("DEBUG")]
        private void WriteToLog(string message)
        {
            if (_cmsLog.IsDebugEnabled)
                _cmsLog.Debug(message);
        }
    }
}
