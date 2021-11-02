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
using Veneka.Module.GviveAPI.Models;
using Veneka.Module.GviveAPI;
using Veneka.Module.CIFIntegration;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityMELCOMFundLoadCBS", "BAF5998D-3401-4410-8CA2-C1FBB61C111E", typeof(ICoreBankingSystem))]
    public class FidelityMELCOMFundsLoadCBS : ICoreBankingSystem
    {
        public IDataSource DataSource { get ; set ; }
        public DirectoryInfo IntegrationFolder { get ; set ; }
        private static readonly ILog _cbsLog = LogManager.GetLogger(General.CBS_LOGGER);
        
        public bool ChargeFee(Integration.Objects.CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string feeRefrenceNumber, out string responseMessage)
        {
            _cbsLog.Debug("Calling Charge fee method in FidelityMELCOMCBS.cs class");

            feeRefrenceNumber = string.Empty;
            //headOfficeFundsLoadAccount
            string fundsLoadAccountMelcom = string.Empty;
            string fundsLoadAccountFidelity = string.Empty;
            //branchFundsLoadAccount
            string branchFundsLoadAccount = string.Empty;
            //TellerAccount
            string tellerAccount = string.Empty;
            //eCashAccount
            string eCashAccount = string.Empty;

            try
            {

                #region pick up accounts configured
                var item = externalFields.Field.FirstOrDefault(i => i.Key == "eCashAccount");
                if (item.Key == null)
                {
                    throw new Exception("eCashAccount external field not found.");
                }
                 eCashAccount = externalFields.Field["eCashAccount"];

                #region accounts configs removed
                //item = externalFields.Field.FirstOrDefault(i => i.Key == "fundsLoadAccountMelcom");
                //if (item.Key == null)
                //{
                //    throw new Exception("fundsLoadAccountMelcom external field not found.");
                //}

                //fundsLoadAccountMelcom = externalFields.Field["fundsLoadAccountMelcom"];

                //item = externalFields.Field.FirstOrDefault(i => i.Key == "fundsLoadAccountFidelity");
                //if (item.Key == null)
                //{
                //    throw new Exception("fundsLoadAccountFidelity external field not found.");
                //}

                //fundsLoadAccountFidelity = externalFields.Field["fundsLoadAccountFidelity"];
                #endregion

                item = externalFields.Field.FirstOrDefault(i => i.Key == "BranchFundsLoadAccount");
                if (item.Key == null)
                {
                    throw new Exception("BranchFundsLoadAccount external field not found.");
                }
                branchFundsLoadAccount = externalFields.Field["BranchFundsLoadAccount"];

                if (!customerDetails.IsCBSAccountHolder)
                {
                    item = externalFields.Field.FirstOrDefault(i => i.Key == "TellerAccount");
                    if (item.Key == null)
                    {
                        throw new Exception("TellerAccount external field not found.");
                    }
                    tellerAccount = externalFields.Field["TellerAccount"];
                }
                #endregion

               //FileLoader.Objects.BranchLookup ll =  DataSource.BranchDAL.GetBranchesForIssuerByIssuerCode("").Where(e => e.BranchId == customerDetails.FundingDetails.);
                
                _cbsLog.Debug("FeeReferencenumber =" + customerDetails.FeeReferenceNumber);
                if (customerDetails.FeeReferenceNumber == null)
                {

                    _cbsLog.Debug("into Charge fee method in FidelityMELCOMCBS.cs class");
                    responseMessage = String.Empty;

                    if (!(config is Config.WebServiceConfig))
                        throw new ArgumentException("CBS config parameters must be for Webservice.");

                    MelcomFlexcubeWebService service = new MelcomFlexcubeWebService((WebServiceConfig)config, DataSource);
                    _cbsLog.Debug("Calling Charge fee service from FidelityCBS.cs class");
                    int currentBranch;
                    //string strBranchCode= DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
                    string strBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);
                    int branchCategory = DataSource.LookupDAL.LookupBranchCategory(customerDetails.BranchId);
                    _cbsLog.Debug("Branch Code : " + strBranchCode);

                    
                    
                    //if (int.TryParse(customerDetails.BranchCode, out currentBranch))
                    if (int.TryParse(strBranchCode, out currentBranch))
                    {
                        //List<string> MelComBranches = melcomBranchCodes.ToList();
                        //if (MelComBranches.Contains(strBranchCode)) //melcome branch
                        if (branchCategory == (int)Util.BranchCategory.MERCHANT_BRANCH)
                        {
                            string username = DataSource.LookupDAL.LookupUserNameById(auditUserId);
                            _cbsLog.Debug(string.Format("Melcom branch ({0}), call appropriate charge fee service", strBranchCode));
                            string debitAcc = customerDetails.IsCBSAccountHolder ? customerDetails.AccountNumber : eCashAccount;
                            _cbsLog.Debug(string.Format("Debit Acc {0}, Fundsload Acc {1}", tellerAccount, fundsLoadAccountFidelity));
                            if (service.ChargeFee(customerDetails, username, debitAcc, branchFundsLoadAccount, languageId,false, out responseMessage))
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
                            //is this fidelity branch
                            string tellerId = DataSource.LookupDAL.LookupBranchTellerIDByUserId(auditUserId);
                            _cbsLog.Debug(string.Format("Fidelity branch ({0}), call appropriate charge fee service", strBranchCode));
                            _cbsLog.Debug(string.Format("Teller Acc {0}, fundsload acc {1}", tellerAccount, fundsLoadAccountFidelity));
                            if (service.ChargeFeeAtFidelityBank(customerDetails, tellerId, tellerAccount, fundsLoadAccountFidelity, branchFundsLoadAccount, languageId,false, out responseMessage))
                            {
                                if (customerDetails.CardId > 0)
                                    DataSource.CardsDAL.UpdateCardFeeReferenceNumber(customerDetails.CardId, customerDetails.FeeReferenceNumber, auditUserId, auditWorkstation);
                                feeRefrenceNumber = customerDetails.FeeReferenceNumber;
                                return true;
                            }
                            else
                                feeRefrenceNumber = string.Empty;
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

        public bool CheckBalance(Integration.Objects.CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        public bool GetAccountDetail_old(string accountNumber, List<IProductPrintField> printFields, int cardIssueReasonId, int issuerId, int branchId, int productId, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out AccountDetails accountDetails, out string responseMessage)
        {
            ////below for workdaround
            responseMessage = string.Empty;

            ////Check in flexcube customer exist or not. If exit continue with prepaid process
            _cbsLog.Trace(t => t("Doing GetAccountDetails"));

            AccountDetails PrepaidAccountDetails = new AccountDetails();
            string branchCode = DataSource.LookupDAL.LookupBranchCode(branchId);
            accountDetails = null;
            accountDetails = BuildAccountDetails(accountNumber);

            _cbsLog.Debug("Done BuildAccount");

            accountDetails.ProductFields = new List<ProductField>();
            _cbsLog.Debug("Calling productFields ");
            foreach (var printField in printFields)
            {
                if (printField is PrintStringField)
                {
                    switch (printField.MappedName.ToLower())
                    {
                        case "ind_sys_dob":
                            ((PrintStringField)printField).Value = "15/06/2002";
                            accountDetails.ProductFields.Add(new ProductField(printField));
                            //_cbsLog.Debug("Date of birth" + dob);
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

            responseMessage = "success";
            return true;


        }
        public bool GetAccountDetail(string accountNumber, List<IProductPrintField> printFields, int cardIssueReasonId, int issuerId, int branchId, int productId, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out AccountDetails accountDetails, out string responseMessage)
        {
            ////below for workdaround
            responseMessage = string.Empty;

            ////Check in flexcube customer exist or not. If exit continue with prepaid process
            _cbsLog.Trace(t => t("Doing GetAccountDetails"));

            #region Auto Feedback LookUp
            //AccountDetails PrepaidAccountDetails = new AccountDetails();
            //string branchCode = DataSource.LookupDAL.LookupBranchCode(branchId);
            //accountDetails = null;
            //accountDetails = BuildAccountDetails(accountNumber);

            //_cbsLog.Debug("Done BuildAccount");

            //accountDetails.ProductFields = new List<ProductField>();
            //_cbsLog.Debug("Calling productFields ");
            //foreach (var printField in printFields)
            //{
            //    if (printField is PrintStringField)
            //    {
            //        switch (printField.MappedName.ToLower())
            //        {
            //            case "ind_sys_dob":
            //                ((PrintStringField)printField).Value = "15/06/2002";
            //                accountDetails.ProductFields.Add(new ProductField(printField));
            //                //_cbsLog.Debug("Date of birth" + dob);
            //                break;
            //            case "ind_sys_address":
            //                ((PrintStringField)printField).Value = "";
            //                accountDetails.ProductFields.Add(new ProductField(printField));
            //                //_cbsLog.Debug("address" + response.adesc);
            //                break;
            //            default: accountDetails.ProductFields.Add(new ProductField(printField)); break;
            //        }

            //        //accountDetails.ProductFields.Add(new ProductField(item));

            //    }
            //}

            //responseMessage = "success";
            //return true;
            #endregion

            if (!(config is Config.WebServiceConfig))
                throw new ArgumentException("CBS config parameters must be for Webservice.");


            string branchCode = DataSource.LookupDAL.LookupBranchCode(branchId);
            _cbsLog.Debug(string.Format("branch id {0}, branch code {1} ", branchId, branchCode));

            accountDetails = null;

            try
            {
                MelcomFlexcubeWebService service = new MelcomFlexcubeWebService((WebServiceConfig)config, DataSource);
                GviveServices gviveService = new GviveServices();
                CIFService amlock = new CIFService();

                //Test
                //string apiKeyValue = "RE38@$HSL%*PP6";
                //string apiKeyName = "Api-Key";

                //string baseUrl = @"https://dpfbgltest01.fidelitybank.com.gh:1504";

                //Prod
                //the prod key
                string apiKeyValue = "OOUY@2912*00EA";
                string apiKeyName = "Api-Key";

                //prod url
                string baseUrl = @"https://intranet";

                #region pick acc number prefix
                var item = externalFields.Field.FirstOrDefault(i => i.Key == "AccNumPrefix");
                if (item.Key == null)
                {
                    throw new Exception("Acc prefix external field not found.");
                }

                string accNumPrefix = externalFields.Field["AccNumPrefix"];
                #endregion

                //return service.QueryCustAcc(accountNumber, printFields, branchCode, languageId, out accountDetails, out responseMessage);
                _cbsLog.Debug("query customer account/id :" + accountNumber);
                if (!service.QueryCustAcc(accountNumber, printFields, branchCode, languageId, accNumPrefix, out accountDetails, out responseMessage))
                {
                    //lets check in Gvive
                    if (accountDetails == null)
                    {
                        _cbsLog.Debug("account detail not found in cbs");
                        var customer = gviveService.GetCustomerDetails(accountNumber, baseUrl, apiKeyValue, apiKeyName).Result;
                        if (!customer.IsSuccess)
                        {
                            _cbsLog.Debug("customer not found in gvive");
                            responseMessage = "customer not found in gvive";
                            return false;
                        }
                        else
                        {
                            _cbsLog.Debug("found person in gvive, lets return the details");
                            //extract information
                            accountDetails = BuildAccountDetails(customer, printFields, accountNumber);
                        }
                    }
                    else
                    {
                        _cbsLog.Debug("account detail has something");
                    }

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
            }

            return false;

        }
        public bool ReverseFee(Integration.Objects.CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        public bool UpdateAccount(Integration.Objects.CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        private AccountDetails BuildAccountDetails(Veneka.Module.GviveAPI.Models.CustomerDetails custDetails, List<ProductPrinting.IProductPrintField> printFields, string searchValue)
        {
            string firstName = string.Empty, middlename = string.Empty, lastname = string.Empty;

            //DecodeName(flexAccountDetails.CUSTNAME, out firstName, out middlename, out lastname);
            if (!string.IsNullOrEmpty(custDetails.Fullname))
            {

                firstName = custDetails.Fullname.Split(' ')[0];
                lastname = custDetails.Fullname.Split(' ')[1];
            }

            if (!string.IsNullOrEmpty(custDetails.FirstName))
            {
                firstName = custDetails.FirstName;
            }
            if (!string.IsNullOrEmpty(custDetails.MiddleName))
            {
                middlename = custDetails.FirstName;
            }
            if (!string.IsNullOrEmpty(custDetails.LastName))
            {
                lastname = custDetails.FirstName;
            }

            AccountDetails rtn = new AccountDetails
            {
                AccountNumber = "MLCM_" + searchValue,//set account number to prefix MLCM_

                //AccountTypeId =-1, // DecodeAccountType(flexAccountDetails.ACCTYPE),
                CurrencyId = DecodeCurrency("GHS"),
                //CBSAccountTypeId=flexAccountDetails.ACCTYPE,

                CBSAccountTypeId = "NotMapped", //DecodeAccType(flexAccountDetails.ACC, flexAccountDetails.ACCTYPE),

                FirstName = string.IsNullOrEmpty(custDetails.FirstName) ? firstName : custDetails.FirstName,// custDetails.FirstName,
                LastName = string.IsNullOrEmpty(custDetails.LastName) ? lastname : custDetails.LastName,//custDetails.LastName,
                MiddleName = string.IsNullOrEmpty(custDetails.MiddleName) ? middlename : custDetails.MiddleName,//lastname,//custDetails.MiddleName,
                CustomerIDNumber = string.IsNullOrEmpty(custDetails.PassportNo) ? searchValue : custDetails.PassportNo,
                Address1 = string.Empty,//flexAccountDetails.ADDR1,
                Address2 = string.Empty,
                Address3 = string.Empty,
                ContactNumber = string.Empty, //custDetails.Custpersonal.MOBNUM.ToString()
                IsCBSAccount = false //acount not in cbs
            };

            _cbsLog.Debug("CBSAccountTypeId==" + rtn.CBSAccountTypeId);
            //-------Start newly added  fields for NI migration----------------
            string dob = DateTime.Now.ToString();
            if (custDetails.DateOfBirth != null)
            {

                dob = custDetails.DateOfBirth.ToString();
                _cbsLog.Debug("Date of birth" + dob);
            }

            string address = string.Empty;
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
                            ((PrintStringField)printField).Value = address;
                            rtn.ProductFields.Add(new ProductField(printField));
                            //_cbsLog.Debug("address" + response.adesc);
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
        private AccountDetails BuildAccountDetails(string ACC)
        {
            //string accountNumber = Convert.ToString(ACC);
           // int accountLength = accountNumber.Length;
            AccountDetails rtn = new AccountDetails
            {
                AccountNumber = "MLC" + ACC,
                CurrencyId = DecodeCurrency("GHS"),
                CBSAccountTypeId = "NotMapped",
                NameOnCard = "MELCOM CUSTOMER",
                FirstName = "MELCOM ",
                MiddleName = "CUSTOMER",
                LastName = "GHANA",
                // IsCBSAccount = false



            };
            _cbsLog.Debug("Account Details CurrencyID==" + rtn.CurrencyId);
            return rtn;
        }
        private int DecodeCurrency(string ccy)
        {
            return DataSource.LookupDAL.LookupCurrency(ccy);

        }
    }
}
