using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.External;
using Veneka.Indigo.Integration.Fidelity.Flexcube;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.ProductPrinting;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityCredtCardCBS", "37359C89-0A42-4998-81C5-D6DEA3646ED1", typeof(ICoreBankingSystem))]
    public class FidelityCredtCardCBS : ICoreBankingSystem
    {

        private static readonly ILog _cbsLog = LogManager.GetLogger(General.CBS_LOGGER);

        public string SQLConnectionString { get; set; }

        public System.IO.DirectoryInfo IntegrationFolder { get; set; }
        public IDataSource DataSource { get; set; }

        public bool UpdateAccount(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {

            //ConfigDAL configDal = new ConfigDAL(this.SQLConnectionString);
            //var rtConfig =   configDal.GetProductInterfaceConfig(customerDetails.ProductId, 5, 1, null, auditUserId, auditWorkstation);
            //if (!(rtConfig is Config.WebServiceConfig))
            //    throw new ArgumentException("Card fees config parameters must be for Webservice.");

            FlexcubeWebService service = new FlexcubeWebService((WebServiceConfig)config, DataSource);

            return service.SetAtmFlag(customerDetails, languageId, out responseMessage);
        }

        public bool CheckBalance(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            try
            {
                responseMessage = String.Empty;

                if (customerDetails.FeeReferenceNumber == null)
                {

                    if (!(config is Config.WebServiceConfig))
                        throw new ArgumentException("CBS config parameters must be for Webservice.");

                    //Get and Check Fee config
                    //var rtConfig = configDal.GetProductInterfaceConfig(customerDetails.ProductId, 5, 1, null, auditUserId, auditWorkstation);
                    //if (!(rtConfig is Config.WebServiceConfig))
                    //    throw new ArgumentException("Card fees config parameters must be for Webservice.");

                    FlexcubeWebService service = new FlexcubeWebService((WebServiceConfig)config, DataSource);

                    if (service.CheckBalance(customerDetails, languageId, out responseMessage))
                    {
                        //cDal.UpdateCardFeeReferenceNumber(customerDetails.CardId, customerDetails.FeeReferenceNumber, auditUserId, auditWorkstation);
                        return true;
                    }
                    else
                        return false;

                }
                return true;
            }

            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cbsLog.Error(endpointException);
                responseMessage = "Unable to connect to Flexcube, please try again or contact support.";
            }
            catch (Exception ex)
            {
                _cbsLog.Error(ex);
                responseMessage = ex.Message;
            }

            return false;
        }

        public bool ChargeFee(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string feeRefrenceNumber, out string responseMessage)
        {
            feeRefrenceNumber = string.Empty;
            if (customerDetails.FeeReferenceNumber == null)
            {
                if (externalFields.Field.FirstOrDefault((KeyValuePair<string, string> i) => i.Key == "cbsProductCode").Key == null)
                {
                    throw new Exception(" CBS Product Code external field not found.");
                }
                string cbsProdCode = externalFields.Field["cbsProductCode"].ToUpper();

                try
                {
                    _cbsLog.Debug($"Calling Charge fee method in FidelityCBS.cs class {customerDetails.AccountNumber}");
                    responseMessage = String.Empty;

                    if (!(config is Config.WebServiceConfig))
                        throw new ArgumentException("CBS config parameters must be for Webservice.");

                    FlexcubeWebService service = new FlexcubeWebService((WebServiceConfig)config, DataSource);
                    _cbsLog.Debug("Calling Charge fee service from FidelityCBS.cs class");
                    if (service.ChargeFee(customerDetails, cbsProdCode, languageId, out responseMessage))
                    {
                        if (customerDetails.CardId > 0)
                            DataSource.CardsDAL.UpdateCardFeeReferenceNumber(customerDetails.CardId, customerDetails.FeeReferenceNumber, auditUserId, auditWorkstation);
                        feeRefrenceNumber = customerDetails.FeeReferenceNumber;
                        return true;
                    }
                    else
                        feeRefrenceNumber = string.Empty;
                    return false;



                }
                catch (System.ServiceModel.EndpointNotFoundException endpointException)
                {
                    _cbsLog.Error(endpointException);
                    responseMessage = "Unable to connect to Flexcube, please try again or contact support.";
                }
                catch (Exception ex)
                {
                    _cbsLog.Error(ex);
                    responseMessage = ex.Message;
                }

                return false;
            }
            responseMessage = string.Empty;
            return true;
        }


        public bool ReverseFee(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;

            try
            {

                if (!(config is Config.WebServiceConfig))
                    throw new ArgumentException("CBS config parameters must be for Webservice.");

                string branchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);

                FlexcubeWebService service = new FlexcubeWebService((WebServiceConfig)config, DataSource);

                //TODO: Reversal??
                //service.ChargeFee(customerDetails.AccountNumber, branchCode, "ccy", 0, languageId, out responseMessage);

                responseMessage = "Success";
                return true;
            }
            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cbsLog.Error(endpointException);
                responseMessage = "Unable to connect to Flexcube, please try again or contact support.";
            }
            catch (Exception ex)
            {
                _cbsLog.Error(ex);
                responseMessage = ex.Message;
            }

            return false;
        }

        public bool GetAccountDetail(string accountNumber, List<IProductPrintField> printFields, int cardIssueReasonId, int issuerId, int branchId, int productId, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out AccountDetails accountDetails, out string responseMessage)
        {
            //Check Core Banking Config
            //var ubsParms = DataSource.ParametersDAL.GetParameterProductInterface(issuerId, 0, 1, null, auditUserId, auditWorkstation);
            if (!(config is Config.WebServiceConfig))
                throw new ArgumentException("CBS config parameters must be for Webservice.");

            string branchCode = DataSource.LookupDAL.LookupBranchCode(branchId);

            accountDetails = null;

            try
            {
                CreditCardFlexcubeWebService service = new CreditCardFlexcubeWebService((WebServiceConfig)config, DataSource);

                return service.QueryCustAcc(accountNumber, printFields, branchCode, languageId, out accountDetails, out responseMessage);
            }
            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cbsLog.Error(endpointException);
                responseMessage = "Unable to connect to Flexcube, please try again or contact support.";
            }

            return false;
        }
    }
}
