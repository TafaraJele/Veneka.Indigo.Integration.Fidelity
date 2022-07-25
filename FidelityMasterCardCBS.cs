using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.External;
using Veneka.Indigo.Integration.Fidelity.Flexcube;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.ProductPrinting;
using Veneka.Module.GviveAPI;
using Veneka.Module.CIFIntegration;
using Newtonsoft.Json;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityMasterCardCBS", "38E46374-3BD9-4E8E-ADB5-764DA7256E60", typeof(ICoreBankingSystem))]
    public class FidelityMasterCardCBS : ICoreBankingSystem
    {
        public IDataSource DataSource { get; set; }
        public DirectoryInfo IntegrationFolder { get; set; }
        private static readonly ILog _cbsLog = LogManager.GetLogger(General.CBS_LOGGER);
        public bool ChargeFee(Integration.Objects.CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string feeRefrenceNumber, out string responseMessage)
        {
            _cbsLog.Debug("Calling Charge fee method in FidelityMELCOMCBS.cs class");

            feeRefrenceNumber = string.Empty;
            //headOfficeFundsLoadAccount
            string headOfficeFundsLoadAccount = string.Empty;
            //branchFundsLoadAccount
            string branchFundsLoadAccount = string.Empty;
            //TellerAccount
            string tellerAccount = string.Empty;
            //eCashAccount
            string eCashAccount = string.Empty;

            string cardIssuingAccountMelcom = string.Empty;
            string cardIssuingAccountFidelity = string.Empty;

            try
            {

                #region pick up accounts configured
                var item = externalFields.Field.FirstOrDefault(i => i.Key == "eCashAccount");
                if (item.Key == null)
                {
                    throw new Exception("eCashAccount external field not found.");
                }
               
                eCashAccount = externalFields.Field["eCashAccount"];

                #region now excluded accounts
                //item = externalFields.Field.FirstOrDefault(i => i.Key == "cardIssuingAccountMelcom");
                //if (item.Key == null)
                //{
                //    throw new Exception("cardIssuingAccountMelcom external field not found.");
                //}
                //cardIssuingAccountMelcom = externalFields.Field["cardIssuingAccountMelcom"];

                //item = externalFields.Field.FirstOrDefault(i => i.Key == "cardIssuingAccountFidelity");
                //if (item.Key == null)
                //{
                //    throw new Exception("cardIssuingAccountFidelity external field not found.");
                //}
                //cardIssuingAccountFidelity = externalFields.Field["cardIssuingAccountFidelity"];
                #endregion
                if (!customerDetails.IsCBSAccountHolder)
                {
                    item = externalFields.Field.FirstOrDefault(i => i.Key == "TellerAccount");
                    if (item.Key == null)
                    {
                        throw new Exception("TellerAccount external field not found.");
                    }
                    tellerAccount = externalFields.Field["TellerAccount"];

                    if(customerDetails.FundingDetails != null)
                    {
                        _cbsLog.Debug("customerDetails.FundingDetails -- Found!!");
                        if (string.IsNullOrEmpty(customerDetails.FundingDetails.TellerId))
                        {
                            _cbsLog.Debug("no teller");

                            customerDetails.FundingDetails.TellerId = DataSource.LookupDAL.LookupBranchTellerIDByUserId(auditUserId);

                            //customerDetails.FundingDetails.TellerId = DataSource.LookupDAL.LookupUserNameById(auditUserId);
                            _cbsLog.Debug("Fetched Teller ID = " + customerDetails.FundingDetails.TellerId);
                        }
                        else
                        {
                            _cbsLog.Debug("Passed Teller ID = " + customerDetails.FundingDetails.TellerId);
                        }
                    }
                    else
                    {
                        _cbsLog.Debug("customerDetails.FundingDetails -- Nothing here");
                    }
                    
                }
                #endregion
                //FileLoader.Objects.BranchLookup ll =  DataSource.BranchDAL.GetBranchesForIssuerByIssuerCode("").Where(e => e.BranchId == customerDetails.FundingDetails.);

                _cbsLog.Debug("FeeReferencenumber =" + customerDetails.FeeReferenceNumber);
                if (customerDetails.FeeReferenceNumber == null)
                {

                    _cbsLog.Debug("into Charge fee method in FidelityMasterCardCBS.cs class");
                    responseMessage = String.Empty;

                    if (!(config is Config.WebServiceConfig))
                        throw new ArgumentException("CBS config parameters must be for Webservice.");

                    MasterCardFlexcubeWebService service = new MasterCardFlexcubeWebService((WebServiceConfig)config, DataSource);
                    _cbsLog.Debug("Calling Charge fee service from FidelityMasterCardCBS.cs class");
                    int currentBranch;
                    //string strBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
                    string strBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);
                    _cbsLog.Debug($"Branch ID to look up is : {customerDetails.BranchId}");
                    int branchCategory = DataSource.LookupDAL.LookupBranchCategory(customerDetails.BranchId);

                    //if (int.TryParse(customerDetails.BranchCode, out currentBranch))
                    if (int.TryParse(strBranchCode, out currentBranch))
                    {
                       
                        if(branchCategory == (int)Util.BranchCategory.BANK_BRANCH)
                        {

                            //is this fidelity branch
                            string tellerId = DataSource.LookupDAL.LookupBranchTellerIDByUserId(auditUserId);
                            _cbsLog.Debug(string.Format("Fidelity branch ({0}), call appropriate charge fee service", strBranchCode));
                            _cbsLog.Debug(string.Format("UserID {0},Teller ID  - {1}", auditUserId, tellerId));

                            if (!customerDetails.IsCBSAccountHolder && customerDetails.FundingDetails == null) 
                            {
                                _cbsLog.Debug("Doing issuance for a non Fidelity customer at Fidelty branch. Stop charging mark card charge fee status as pending");

                                //mark card charge fee as PENDING charging for charging at the teller
                                int cardChargeFeeStatus = 0;
                                DataSource.CardsDAL.UpdateCardChargeFeeStatus(customerDetails.CardId, cardChargeFeeStatus, auditUserId, auditWorkstation);
                                return true;
                            }

                            if (service.ChargeFeeAtFidelityBank(customerDetails,tellerId, tellerAccount, cardIssuingAccountFidelity, branchFundsLoadAccount, languageId, true, out responseMessage))
                            {
                                if (customerDetails.CardId > 0)
                                    DataSource.CardsDAL.UpdateCardFeeReferenceNumber(customerDetails.CardId, customerDetails.FeeReferenceNumber, auditUserId, auditWorkstation);
                                feeRefrenceNumber = customerDetails.FeeReferenceNumber;
                                return true;
                            }
                            else
                                feeRefrenceNumber = string.Empty;
                        }
                        else
                        {
                            responseMessage = string.Format("Issuing for product {0}, not allow in branch {1}  ", customerDetails.ProductCode, strBranchCode);
                        }
                    }
                    else
                    { 
                        _cbsLog.Debug(string.Format("ChargeFee --> Check Branch Code not in expected format. Branch Code is {0}", strBranchCode));
                    }
                    #region previous code
                    //if (service.ChargeFee(customerDetails, eCashAccount, fundsLoadAccount, languageId, out responseMessage))
                    //{
                    //    if (customerDetails.CardId > 0)
                    //        DataSource.CardsDAL.UpdateCardFeeReferenceNumber(customerDetails.CardId, customerDetails.FeeReferenceNumber, auditUserId, auditWorkstation);
                    //    feeRefrenceNumber = customerDetails.FeeReferenceNumber;
                    //    return true;
                    //}
                    //else
                    //    feeRefrenceNumber = string.Empty;
                    #endregion
                    return false;
                }

                responseMessage = string.Empty;
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
        public bool ChargeFee_old(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string feeRefrenceNumber, out string responseMessage)
        {
            string productTag = "MASC";
            var item = externalFields.Field.FirstOrDefault(i => i.Key == "eCashAccount");
            if (item.Key == null)
            {
                throw new Exception("eCashAccount external field not found.");
            }
            string eCashAccount = externalFields.Field["eCashAccount"];
            item = externalFields.Field.FirstOrDefault(i => i.Key == "cardIssuingAccount");
            if (item.Key == null)
            {
                throw new Exception("cardIssuingAccount external field not found.");
            }
            string cardIssuingAccount = externalFields.Field["cardIssuingAccount"];
            feeRefrenceNumber = string.Empty;
            if (customerDetails.FeeReferenceNumber == null)
            {
                try
                {
                    _cbsLog.Debug("Calling Charge fee method in FidelityMELCOMCBS.cs class");
                    responseMessage = String.Empty;

                    if (!(config is Config.WebServiceConfig))
                        throw new ArgumentException("CBS config parameters must be for Webservice.");



                    MelcomFlexcubeWebService service = new MelcomFlexcubeWebService((WebServiceConfig)config, DataSource);
                    _cbsLog.Debug("Calling Charge fee service from FidelityCBS.cs class");
                    if (service.ChargeFee(customerDetails,"", eCashAccount, cardIssuingAccount, languageId,true,productTag, out responseMessage))
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

    public bool CheckBalance(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;

        }

        public bool GetAccountDetail(string accountNumber, List<IProductPrintField> printFields, int cardIssueReasonId, int issuerId, int branchId, int productId, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out AccountDetails accountDetails, out string responseMessage)
        {
            _cbsLog.Trace(t => t($"Doing GetAccountDetails {accountNumber}"));

            string ghanaCardCode = string.Empty;
            string redirectUrl = string.Empty;

            var accountIndex = accountNumber.IndexOf(';');
            if (accountIndex != -1)
            {
                var stringArray = accountNumber.Split(';');
                accountNumber = stringArray[0];
                redirectUrl = stringArray[1];
                ghanaCardCode = stringArray[2];

                _cbsLog.Trace($"ghanaCardCode {ghanaCardCode} redirectUrl {redirectUrl} account Number {accountNumber}");
            }

            ////below for workdaround
            responseMessage = string.Empty;
            #region old code


            #endregion
            //below correct method for melcom//

            if (!(config is Config.WebServiceConfig))
                throw new ArgumentException("CBS config parameters must be for Webservice.");


            string branchCode = DataSource.LookupDAL.LookupBranchCode(branchId);

            _cbsLog.Debug(string.Format("branch id {0}, branch code {1} ", branchId, branchCode));

            accountDetails = null;

            Product _product = DataSource.ProductDAL.GetProduct(productId, true, auditUserId, auditWorkstation);


            try
            {
                MasterCardFlexcubeWebService service = new MasterCardFlexcubeWebService((WebServiceConfig)config, DataSource);
                GviveServices gviveService = new GviveServices();
                CIFService cifservice = new CIFService();

                ////the test key
                //string apiKeyValue = "RE38@$HSL%*PP6";
                //string apiKeyName = "Api-Key";

                ////the prod key
                string apiKeyValue = "OOUY@2912*00EA";
                string apiKeyName = "Api-Key";

                //test url
                //string baseUrl = @"https://dpfbgltest01.fidelitybank.com.gh:1504";

                //prod url
                string baseUrl = @"https://intranet";
                //string baseUrl = @"https://gvivegh.com:1355";


                #region pick acc number prefix
                var item = externalFields.Field.FirstOrDefault(i => i.Key == "AccNumPrefix");
                if (item.Key == null)
                {
                    throw new Exception("Acc prefix external field not found.");
                }

                string accNumPrefix = externalFields.Field["AccNumPrefix"];
                #endregion
                accountDetails = null;
                //return service.QueryCustAcc(accountNumber, printFields, branchCode, languageId, out accountDetails, out responseMessage);
                _cbsLog.Debug("query customer account/id :" + accountNumber);
                if (!service.QueryCustAcc(accountNumber, printFields, branchCode, languageId, accNumPrefix, out accountDetails, out responseMessage))
                {
                    //lets check in Ghana card
                    if (accountDetails == null && !string.IsNullOrEmpty(ghanaCardCode) && !string.IsNullOrEmpty(redirectUrl))
                    {
                        _cbsLog.Debug($"account detail not found in cbs get token for ghana card search ghanaCardCode {ghanaCardCode} redirectUrl {redirectUrl}");

                        var ghanaCardToken = cifservice.GetGhanaCardAuthToken(ghanaCardCode, @"http://localhost:453/fileprocessing/api/customers", redirectUrl).Result;

                        if (ghanaCardToken.ResponseType == GhanaCard.Module.Enums.ResponseType.SUCCESSFUL)
                        {
                            var personDetails = cifservice.GhanaCardGetPersonInfo(ghanaCardToken.Value, @"http://localhost:453/fileprocessing/api/customers").Result;
                            if (personDetails.ResponseType == GhanaCard.Module.Enums.ResponseType.UNSUCCESSFUL)
                            {
                                _cbsLog.Debug("customer not found in Ghana card");
                                responseMessage = "customer not found in ghana card";
                                return false;
                            }
                            else
                            {
                                _cbsLog.Debug("found person in ghana card, lets return the details");
                                //extract information
                                accountDetails = BuildAccountDetails(personDetails.Value, printFields, accountNumber, accNumPrefix);

                                #region CIF - Amlock Validation
                                string indigoReference = String.Format("Am{0}{1}", DateTime.Now.ToString("yy"),
                                                                        DateTime.Now.DayOfYear);
                                indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("amlocktxn", ResetPeriod.DAILY)
                                                .ToString().PadLeft((16 - indigoReference.Length), '0');
                                var customer = personDetails.Value.Data;
                                Veneka.Module.CIFIntegration.Models.Prospect prospect = new Module.CIFIntegration.Models.Prospect()
                                {
                                    dateOfBirth = customer.BirthDate,
                                    nationalId = customer.NationalId,
                                    otherNames = customer.Forenames,
                                    surname = customer.Surname,
                                    id = indigoReference,
                                    nationality = customer.Nationality,
                                    pep = "N"
                                };

                                //var resp = amlock.AmlockValidation(@"http://10.179.143.120:453/fileprocessing/api/customers/amlock-validation", prospect).Result;
                                var resp = cifservice.AmlockValidation(@"http://localhost:453/fileprocessing/api/customers/amlock-validation", prospect).Result;

                                if (resp.statusCode != Module.CIFIntegration.Enums.StatusCode.Ok)
                                {
                                    responseMessage = $"Invalid Message. Response error {resp.message}";
                                    return false;
                                }
                                else
                                {
                                    if (resp.responseCode == "01")
                                    {
                                        responseMessage = $"Customer {accountDetails.LastName} {accountDetails.FirstName} is black listed."; //$"Invalid Message for request {resp.prospectId}. Response error {resp.message}";
                                        return false;
                                    }
                                    else if (resp.responseCode == "00")
                                    {
                                        return true;
                                    }
                                }

                                return true;
                                #endregion
                            }
                        }

                        //var customer = gviveService.GetCustomerDetails(accountNumber, baseUrl, apiKeyValue, apiKeyName).Result;

                    }
                    else
                    {
                        _cbsLog.Debug("account detail has something");

                    }

                }

                _cbsLog.Debug("QueryCustAcc() returned Is account found in CBS --> " + accountDetails.IsCBSAccount);
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
            }

            return false;

            ////till above//
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

       
        private string DecodeAccType(string accountNum, string accountType)
        {
            if (accountNum.Trim().StartsWith("1"))
            {
                accountType = "U";
                _cbsLog.Debug("Account Type==" + accountType);
            }
            if (accountNum.Trim().StartsWith("2"))
            {
                accountType = "S";
                _cbsLog.Debug("Account Type==" + accountType);
            }
            return accountType;
        }
        private AccountDetails BuildAccountDetails(Persondata personData, List<ProductPrinting.IProductPrintField> printFields, string searchValue, string accNumPrefix)
        {
            string firstName = string.Empty, middlename = string.Empty, lastname = string.Empty;

            var custDetails = personData.Data;

            var namesArray = new string[2];
            //DecodeName(flexAccountDetails.CUSTNAME, out firstName, out middlename, out lastname);

            var firstnameIndex = custDetails.Forenames.IndexOf(' ');
            if (firstnameIndex != -1)
            {
                namesArray = GetNames(custDetails.Forenames);
                firstName = namesArray[0];
                middlename = namesArray[1];
                lastname = custDetails.Surname;

            }
            else
            {
                firstName = custDetails.Forenames;
                lastname = custDetails.Surname;
            }

            string addressValue = string.Empty;
            foreach (var personaddress in custDetails.Addresses)
            {
                if (personaddress.GpsAddressDetails != null)
                {
                    var _address = personaddress.GpsAddressDetails;
                    addressValue = $"{_address.Street} {_address.Area} {_address.District} {_address.Region}";
                    _cbsLog.Debug($"Address {addressValue}");
                }
            }

            string customerContact = string.Empty;
            foreach (var contactNumber in custDetails.Contact.PhoneNumbers)
            {
                customerContact = contactNumber.PhoneNumber;
                _cbsLog.Debug($"Customer contact {customerContact}");
            }

            AccountDetails rtn = new AccountDetails
            {
                AccountNumber = string.Format("{0}_{1}", accNumPrefix, searchValue),
                // AccountNumber = "MLCM_" + searchValue,//set account number to prefix MLCM_

                //AccountTypeId =-1, // DecodeAccountType(flexAccountDetails.ACCTYPE),
                CurrencyId = DecodeCurrency("GHS"),
                //CBSAccountTypeId=flexAccountDetails.ACCTYPE,

                CBSAccountTypeId = "NotMapped", //DecodeAccType(flexAccountDetails.ACC, flexAccountDetails.ACCTYPE),

                FirstName = firstName,// custDetails.FirstName,
                LastName = lastname,//custDetails.LastName,
                MiddleName = middlename,//lastname,//custDetails.MiddleName,
                CustomerIDNumber = custDetails.NationalId,
                Address1 = addressValue,//flexAccountDetails.ADDR1,
                Address2 = string.Empty,
                Address3 = string.Empty,
                ContactNumber = customerContact, //custDetails.Custpersonal.MOBNUM.ToString()
                IsCBSAccount = false
            };

            _cbsLog.Debug("CBSAccountTypeId==" + rtn.CBSAccountTypeId);
            //-------Start newly added  fields for NI migration----------------
            string dob = DateTime.Now.ToString();
            if (custDetails != null)
            {
                dob = custDetails.BirthDate;
                _cbsLog.Debug("Date of birth" + dob);
            }


            rtn.ProductFields = new List<ProductField>();

            foreach (var printField in printFields)
            {
                if (printField is PrintStringField)
                {
                    switch (printField.MappedName.ToLower())
                    {
                        case "ind_sys_dob":
                            ((PrintStringField)printField).Value = dob;
                            rtn.ProductFields.Add(new ProductField(printField));
                            _cbsLog.Debug("Date of birth" + dob);
                            break;
                        case "ind_sys_address":
                            ((PrintStringField)printField).Value = addressValue;
                            rtn.ProductFields.Add(new ProductField(printField));
                            _cbsLog.Debug("ind_sys_address" + addressValue);
                            break;
                        case "ind_sys_postcode":
                            ((PrintStringField)printField).Value = "0000";
                            rtn.ProductFields.Add(new ProductField(printField));
                            break;
                        default: rtn.ProductFields.Add(new ProductField(printField)); break;
                    }

                }



            }

            return rtn;
        }

        private string[] GetNames(string forenames)
        {
            var firstnameIndex = forenames.IndexOf(' ');
            var firstName = forenames.Substring(0, firstnameIndex);
            var othernames = forenames.Substring(firstnameIndex + 1);

            return new string[] { firstName, othernames };
        }
        private int DecodeCurrency(string ccy)
        {
            return DataSource.LookupDAL.LookupCurrency(ccy);

        }
    }
}
