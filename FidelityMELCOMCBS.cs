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
    [IntegrationExport("FidelityMELCOMCBS", "E8FE197B-3587-4440-982B-8337B0F68F61", typeof(ICoreBankingSystem))]
    public class FidelityMELCOMCBS : ICoreBankingSystem
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

                    _cbsLog.Debug("into Charge fee method in FidelityMELCOMCBS.cs class");
                    responseMessage = String.Empty;

                    if (!(config is Config.WebServiceConfig))
                        throw new ArgumentException("CBS config parameters must be for Webservice.");

                    MelcomFlexcubeWebService service = new MelcomFlexcubeWebService((WebServiceConfig)config, DataSource);
                    _cbsLog.Debug("Calling Charge fee service from FidelityCBS.cs class");
                    int currentBranch;
                    //string strBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
                    string strBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);


                    _cbsLog.Debug($"Branch ID to look up is : {customerDetails.BranchId}");
                    int branchCategory = DataSource.LookupDAL.LookupBranchCategory(customerDetails.BranchId);

                    if (int.TryParse(strBranchCode, out currentBranch))
                    {
                       // List<string> MelComBranches = melcomBranchCodes.ToList();
                       // if (MelComBranches.Contains(strBranchCode)) //melcome branch
                        if (branchCategory == (int)Util.BranchCategory.MERCHANT_BRANCH)
                         {
                            
                            _cbsLog.Debug(string.Format("Melcom branch ({0}), call appropriate charge fee service", strBranchCode));
                            //22/07/2021 : Musa - Update account to debit based on whether customer is Fidelity customer or not
                            string accountToDebit = customerDetails.IsCBSAccountHolder ? customerDetails.AdditionalAccountNumber : eCashAccount;
                            _cbsLog.Debug($"At branch {strBranchCode} account to debit is {accountToDebit}");
                            string username = DataSource.LookupDAL.LookupUserNameById(auditUserId);
                            _cbsLog.Debug(string.Format("UserID {0},Teller ID  - {1}", auditUserId, username));
                            if (service.ChargeFee(customerDetails, username, accountToDebit, cardIssuingAccountMelcom, languageId, true, out responseMessage))
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
                            _cbsLog.Debug(string.Format("UserID {0},Teller ID  - {1}", auditUserId, tellerId));
                            _cbsLog.Debug(string.Format("Fidelity branch ({0}), call appropriate charge fee service", strBranchCode));
                            if (service.ChargeFeeAtFidelityBank(customerDetails, tellerId, tellerAccount, cardIssuingAccountFidelity, branchFundsLoadAccount, languageId, true, out responseMessage))
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

        public bool ChargeFee_old(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkstation, out string feeRefrenceNumber, out string responseMessage)
        {
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
                    if (service.ChargeFee(customerDetails,"", eCashAccount, cardIssuingAccount, languageId,true, out responseMessage))
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
            ////below for workdaround
            responseMessage = string.Empty;

            ////Check in flexcube customer exist or not. If exit continue with prepaid process
            //_cbsLog.Trace(t => t("Doing GetAccountDetails"));

            //AccountDetails PrepaidAccountDetails = new AccountDetails();
            //string branchCode = DataSource.LookupDAL.LookupBranchCode(branchId);
            //accountDetails = null;
            //accountDetails = BuildAccountDetails(Convert.ToInt32(accountNumber));

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


            //below correct method for melcom//

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

                //the test key
                string apiKeyValue = "RE38@$HSL%*PP6";
                string apiKeyName = "Api-Key";

                //the prod key
                //string apiKeyValue = "OOUY@2912*00EA";
                //string apiKeyName = "Api-Key";

                //test url
                string baseUrl = @"https://dpfbgltest01.fidelitybank.com.gh:1504";

                //prod url
                //string baseUrl = @"https://intranet";
                //string baseUrl = @"https://gvivegh.com:1355";


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
                            accountDetails = BuildAccountDetails(customer, printFields, accountNumber, accNumPrefix);

                            #region CIF - Amlock Validation
                            string indigoReference = String.Format("Am{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);
                            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("amlocktxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

                            Veneka.Module.CIFIntegration.Models.Prospect prospect = new Module.CIFIntegration.Models.Prospect()
                            {
                                dateOfBirth = customer.DateOfBirth,
                                nationalId = accountNumber,
                                otherNames = accountDetails.FirstName + " " + accountDetails.MiddleName,
                                surname = accountDetails.LastName,
                                id = indigoReference,
                                nationality = customer.Nationality,
                                pep = "N"
                            };
                            
                            //var resp = amlock.AmlockValidation(@"http://10.179.143.120:453/fileprocessing/api/customers/amlock-validation", prospect).Result;
                            var resp = amlock.AmlockValidation(@"http://localhost:453/fileprocessing/api/customers/amlock-validation", prospect).Result;

                            var data = JsonConvert.SerializeObject(resp);
                            _cbsLog.Info($"Amlock response {data}");

                            if (resp.statusCode != Module.CIFIntegration.Enums.StatusCode.Ok)
                            {
                                _cbsLog.Info($"show error response ");
                                if (resp.errorMessages.Count > 0)
                                {
                                    StringBuilder message = new StringBuilder();
                                    foreach (var errorMessage in resp.errorMessages)
                                    {
                                        message.Append(errorMessage.message);
                                    }
                                    if (message.ToString() == "Amlock error, check logs for exception details")
                                    {
                                        responseMessage = $"{message}";
                                        return false;
                                    }
                                    else
                                    {
                                        responseMessage = $"Error! {message}, {customer.SearchType.ToString()} not supported use another ID type ";
                                        return false;
                                    }

                                }
                                if (!string.IsNullOrEmpty(resp.exceptionMessage))
                                {
                                    responseMessage = $"{resp.exceptionMessage}";
                                    return false;
                                }

                                responseMessage = "Error!!!";
                                return false;
                            }
                            else
                            {
                                _cbsLog.Info($"show success response ");

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
                            #endregion
                        }
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
            private AccountDetails BuildAccountDetails(Veneka.Module.GviveAPI.Models.CustomerDetails custDetails, List<ProductPrinting.IProductPrintField> printFields, string searchValue,string accNumPrefix)
        {
            string firstName = string.Empty, middlename = string.Empty, lastname = string.Empty;

            //DecodeName(flexAccountDetails.CUSTNAME, out firstName, out middlename, out lastname);
            if (!string.IsNullOrEmpty(custDetails.Fullname))
            {
                firstName = custDetails.Fullname.Split(' ')[0];
                lastname = custDetails.Fullname.Split(' ')[1];

                //get customer other names
                var customerNames = custDetails.Fullname.Split(' ');
                StringBuilder builder = new StringBuilder();
                if (customerNames.Length > 1)
                {
                    for (int i = 2; i < (customerNames.Length); i++)
                    {
                        builder.Append($"{customerNames[i]} ");
                    }
                    middlename = builder.ToString();
                }
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
                AccountNumber = string.Format("{0}_{1}", accNumPrefix, searchValue),
               // AccountNumber = "MLCM_" + searchValue,//set account number to prefix MLCM_

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
                IsCBSAccount = false
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

        private int DecodeCurrency(string ccy)
        {
            return DataSource.LookupDAL.LookupCurrency(ccy);

        }
    }
}
