using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Objects;
using Common.Logging;
using Veneka.Module.OracleFlexcube;
using Veneka.Indigo.Integration.DAL;
using System.Reflection;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.Fidelity.DAL;
using Veneka.Indigo.Integration.ProductPrinting;

namespace Veneka.Indigo.Integration.Fidelity.Flexcube
{
    public class FlexcubeWebService
    {
        #region Private Fields
        private static readonly ILog _cbsLog = LogManager.GetLogger(General.CBS_LOGGER);
        private static readonly ILog _atmupdateflagLog = LogManager.GetLogger(General.ATMFLAGUPDATE_LOGGER);


        private readonly AccountServicesValidated _flexAccService;
        private readonly RTServicesValidated _flexRtService;
        private readonly CustomerServiceValidated _flexCustService;
        private readonly WebServiceConfig _ubsParms;
        private readonly WebServiceConfig _rtParms;
      
        
        public IDataSource DataSource { get; set; }
        private readonly string _connectionString;
        #endregion

        #region Constructors
        public FlexcubeWebService(Config.WebServiceConfig parameters, IDataSource _dataSource)
        {
            
            string fullPath = parameters.Path;        
            string[] details = fullPath.Split(';');     
            string flexAccServicePath = string.Empty;
            string flexRtServicePath = string.Empty;
            string flexCustServicePath = string.Empty;
     
            if (details == null)
            {
                throw new ArgumentNullException("path is not configured properly for account and fee service separated with ;");
            }
            else
            {
                if (details.Length == 3)
                {
                    flexAccServicePath = details[0];
                    flexRtServicePath = details[1];
                    flexCustServicePath = details[2];

                }
                else
                {
              
                    throw new ArgumentException("path is not configured properly for account and fee service separated with ;");
                }
            }

            //this._ubsParms = ubsParamters;
            //this._rtParms = rtParamters;
            
            DataSource = _dataSource;
            AccountServicesValidated.Protocol ubsProtocol = AccountServicesValidated.Protocol.HTTPS;

            switch (parameters.Protocol)
            {
                case Protocol.HTTP: ubsProtocol = AccountServicesValidated.Protocol.HTTP; break;
                case Protocol.HTTPS: ubsProtocol = AccountServicesValidated.Protocol.HTTPS; break;
                default: break;
            }

            //Initialise Flexcube Account Services
            _flexAccService = new AccountServicesValidated(ubsProtocol, parameters.Address, parameters.Port, flexAccServicePath,
                                                 parameters.Timeout, Veneka.Module.OracleFlexcube.ServicesValidated.Authentication.NONE, parameters.Username, parameters.Password, parameters.Nonce, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), General.CBS_LOGGER);

            RTServicesValidated.Protocol rtProtocol = RTServicesValidated.Protocol.HTTPS;

            switch (parameters.Protocol)
            {
                case Protocol.HTTP: rtProtocol = RTServicesValidated.Protocol.HTTP; break;
                case Protocol.HTTPS: rtProtocol = RTServicesValidated.Protocol.HTTPS; break;
                default: break;
            }

            //Initialise Flexcube RealTime Services
            //"/FCUBSRTService/FCUBSRTService"
            _flexRtService = new RTServicesValidated(rtProtocol, parameters.Address, parameters.Port, flexRtServicePath,
                                                 parameters.Timeout, Veneka.Module.OracleFlexcube.ServicesValidated.Authentication.NONE, parameters.Username, parameters.Password, parameters.Nonce,new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), General.CBS_LOGGER);

            CustomerServiceValidated.Protocol custProtocol = CustomerServiceValidated.Protocol.HTTPS;
            switch (parameters.Protocol)
            {
                case Protocol.HTTP: custProtocol = CustomerServiceValidated.Protocol.HTTP; break;
                case Protocol.HTTPS: custProtocol = CustomerServiceValidated.Protocol.HTTPS; break;
                default: break;
            }
            _flexCustService = new CustomerServiceValidated(custProtocol, parameters.Address, parameters.Port, flexCustServicePath,
                parameters.Timeout, Veneka.Module.OracleFlexcube.ServicesValidated.Authentication.NONE, parameters.Username, parameters.Password, parameters.Nonce, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), General.CBS_LOGGER);

        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Queries Flexcube for the desired customers account and balance
        /// </summary>
        /// <param name="acc"></param>
        public bool QueryCustAcc(string accountNumber, List<ProductPrinting.IProductPrintField> printFields, string branchCode, int languageId, out AccountDetails accDetails, out string responseMessage)
        {
            _flexAccService.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            accDetails = null;
            Veneka.Module.OracleFlexcube.UBSAccWebService.CustAccountFullType custAcc;

            //Build unique indigo reference for this transaction
            string indigoReference = String.Format("ICI{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

            //Step 1: Find the customer account
            if (!_flexAccService.QueryCustomerAccount(accountNumber, branchCode, indigoReference, out custAcc, out messages))
            {
                //Check if the custAcc is null
                responseMessage = BuildHtmlMessage(messages);
                return false;
                
            }

            // Step 2 :Look for customer DOB

            string customerNum = custAcc.CUSTNO;
            Veneka.Module.OracleFlexcube.UBSCustWebService.CustomerFullType custDetails;
            List<Tuple<string, string>> custDetailsmessages;

            if (!_flexCustService.QueryCustomerDetails(customerNum,branchCode, indigoReference, out custDetails, out custDetailsmessages))
            {
                //Check if the custDetails is null
                responseMessage = BuildHtmlMessage(custDetailsmessages);
                return false;
            }


            accDetails = BuildAccountDetails(custAcc, printFields,custDetails);

            //Veneka.Module.OracleFlexcube.UBSAccWebService.AccBalRestypeACC_BAL accBalance;

            //Step 2: Fetch customers balance
            // disabled for testing
            //if (!_flexAccService.QueryAccountBalance(accountNumber, branchCode, out accBalance, out messages))
            //{
            //    responseMessage = BuildHtmlMessage(messages);
            //    return false;
            //}

            ////Check that the currency of the account and the balance match.
            //if (DecodeCurrency(accBalance.CCY) != accDetails.CurrencyId)
            //{
            //    responseMessage = String.Format("Currency of account ({0}) does not match that of account returned in balance query ({1}).", custAcc.CCY, accBalance.CCY);
            //    return false;
            //}

            //accDetails.AccountBalance = accBalance.AVLBAL;
            responseMessage = "";
            return true;
        }

        public bool CheckBalance(CustomerDetails customerDetails, int languageId, out string responseMessage)
        {
            List<Tuple<string, string>> messages;
            responseMessage = String.Empty;

           // LookupDAL lDal = new LookupDAL(_connectionString);
            string branchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
            string ccy = encodeCurrency(customerDetails.CurrencyId.Value);

            Veneka.Module.OracleFlexcube.UBSAccWebService.AccBalRestypeACC_BAL accBalance;

            //Build unique indigo reference for this transaction
            string indigoReference = String.Format("ICI{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

            //Step 1: Check that account has enough funds
            _cbsLog.Debug($"query account balance this is Check balance method. Account number {customerDetails.AccountNumber}");

            if (!_flexAccService.QueryAccountBalance(customerDetails.AccountNumber, branchCode, indigoReference, out accBalance, out messages))
            {
                responseMessage = BuildHtmlMessage(messages);
                return false;
            }

            if (!accBalance.CCY.Trim().ToUpper().Equals(ccy.Trim().ToUpper()))
            {
                responseMessage = String.Format("Currency of transaction {0} does not match that of account {1}", ccy, accBalance.CCY);
                return false;
            }

            if (accBalance.AVLBAL < customerDetails.FeeCharge.Value)
            {
                responseMessage = String.Format("Available balance of the account not sufficient.", ccy, accBalance.CCY);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method will charge a fee to the selected customer.
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="branchCode"></param>
        /// <param name="ccy"></param>
        /// <param name="transactionAmount"></param>
        /// <param name="languageId"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public bool ChargeFee(CustomerDetails customerDetails, int languageId, out string responseMessage)
        {
            _flexRtService.LanguageId = languageId;
            List<Tuple<string, string>> messages;

           // LookupDAL lDal = new LookupDAL(_connectionString);
            string branchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
            string chargeBranchCode = branchCode;
            if (customerDetails.ChargeFeeToIssuingBranch)
                chargeBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);

            string ccy = encodeCurrency(customerDetails.CurrencyId.Value);

            Veneka.Module.OracleFlexcube.UBSAccWebService.AccBalRestypeACC_BAL accBalance;
            _cbsLog.Debug("Calling Charge fee method in FlexcubeWebService class");

            //Build unique indigo reference for this transaction
            string indigoReference1 = String.Format("ICI{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference1 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference1.Length), '0');
            //Step 1: Check that account has enough funds
            _cbsLog.Debug($"This is Charge Fee method before FlexAccService QueryAccountBalance for account {customerDetails.AccountNumber} ");

            if (!_flexAccService.QueryAccountBalance(customerDetails.AccountNumber, branchCode, indigoReference1,out accBalance, out messages))
            {
                responseMessage = BuildHtmlMessage(messages);
                return false;
            }
            _cbsLog.Debug("Done with account balance");
            if (!accBalance.CCY.Trim().ToUpper().Equals(ccy.Trim().ToUpper()))
            {
                responseMessage = String.Format("Currency of transaction {0} does not match that of account {1}", ccy, accBalance.CCY);
                return false;
            }

            if (accBalance.AVLBAL < customerDetails.FeeCharge.Value)
            {
                responseMessage = String.Format("Available balance of the account not sufficient.", ccy, accBalance.CCY);
                return false;
            }
            _cbsLog.Debug("Done with account balance and is greater than the chargefee");
            //Build unique indigo reference for this transaction
            string indigoReference = String.Format("ICI{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

            customerDetails.FeeReferenceNumber = indigoReference;

            string MessageId = String.Format("BIL{0}{1}", DateTime.Now.ToString("yy"),
                                                                   DateTime.Now.DayOfYear);

            MessageId += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - MessageId.Length), '0');
            if (indigoReference.Length > 16)
            {
                responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigoReference);
                return false;
            }

            string flexcubeReference;
           
            if (!_flexRtService.CreateTransactionFS(customerDetails.AccountNumber, chargeBranchCode, branchCode, indigoReference, ccy, customerDetails.FeeCharge.Value, MessageId, out flexcubeReference, out messages))
            {
                responseMessage = BuildHtmlMessage(messages);
                return false;
            }
          
            customerDetails.FeeReferenceNumber = flexcubeReference;
            _cbsLog.Debug(" customerDetails.FeeReferenceNumber==" + customerDetails.FeeReferenceNumber);
            responseMessage = String.Empty;
            return true;
        }

        public bool ReverseFee(string accountNumber, string branchCode, string ccy, decimal transactionAmount, int languageId, out string responseMessage)
        {
            _flexRtService.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            responseMessage = String.Empty;
            return true;
        }

        public bool SetAtmFlag(CustomerDetails customerDetails, int languageId, out string responseMessage)
        {
            List<Tuple<string, string>> messages;
            responseMessage = String.Empty;
            bool resp = false;

            //LookupDAL lDal = new LookupDAL(_connectionString);
            string branchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
            string branchName = DataSource.LookupDAL.LookupBranchName(customerDetails.DomicileBranchId);

            Module.OracleFlexcube.UBSAccWebService.CustAccountFullType custAccount;
            if (_flexAccService.QueryCustomerAccount(customerDetails.AccountNumber, branchCode,"CVFDERWERT123456", out custAccount, out messages))
            {
                if (custAccount.ATM.ToUpper() == "N")
                {
                    custAccount.ATM = "Y";
                    //Build unique indigo reference for this transaction
                    string indigoReference = String.Format("ICI{0}{1}", DateTime.Now.ToString("yy"),
                                                                            DateTime.Now.DayOfYear);

                    indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                                    .ToString().PadLeft((16 - indigoReference.Length), '0');
                    resp = _flexAccService.ModifyCustomerAccount(custAccount, branchCode, indigoReference, out messages);
                    responseMessage = BuildHtmlMessage(messages);
                }
                else
                {
                    responseMessage = "ATM Flag is already enabled for this customer.";
                    _atmupdateflagLog.Trace(string.Format("Acc:{0}, \n branch : {1},\n Date and time:{2},  \n Reason:{3}", customerDetails.AccountNumber, branchCode + "-" + branchName, DateTime.Now.ToString(), responseMessage));

                    return true;
                }
            }



            if (!resp)
            {
                _atmupdateflagLog.Trace(string.Format("Acc:{0}, \n branch : {1},\n Date and time:{2},  \n Reason for failure:{3}", customerDetails.AccountNumber, branchCode + "-" + branchName, DateTime.Now.ToString(), responseMessage));
            }
            return resp;
        }
        #endregion

        #region Private Methods

        private AccountDetails BuildAccountDetails(Veneka.Module.OracleFlexcube.UBSAccWebService.CustAccountFullType flexAccountDetails, List<ProductPrinting.IProductPrintField> printFields, Veneka.Module.OracleFlexcube.UBSCustWebService.CustomerFullType custDetails)
        {
            string firstName, middlename, lastname;

            DecodeName(flexAccountDetails.CUSTNAME, out firstName, out middlename, out lastname);
            
            AccountDetails rtn = new AccountDetails
            {
                AccountNumber = flexAccountDetails.ACC,

                //AccountTypeId =-1, // DecodeAccountType(flexAccountDetails.ACCTYPE),
                CurrencyId = DecodeCurrency(flexAccountDetails.CCY),
                //CBSAccountTypeId=flexAccountDetails.ACCTYPE,
                CBSAccountTypeId = DecodeAccType(flexAccountDetails.ACC,flexAccountDetails.ACCTYPE),
                FirstName = firstName,
                LastName = lastname,
                MiddleName = middlename,
                Address1 = flexAccountDetails.ADDR1,
                Address2 = flexAccountDetails.ADDR2,
                Address3 = flexAccountDetails.ADDR3,
                


            };
            _cbsLog.Debug("CBSAccountTypeId==" + rtn.CBSAccountTypeId);
            //-------Start newly added  fields for NI migration----------------
            string dob = DateTime.Now.ToString();
            if (custDetails.Custpersonal.DOB !=null)
            {
                dob = custDetails.Custpersonal.DOB.ToString();
                _cbsLog.Debug("Date of birth" + dob);
            }
            else
            if  (custDetails.Custcorp.INCORPDT!=null)
            {
                dob = custDetails.Custcorp.INCORPDT.ToString();
                _cbsLog.Debug("Date incorporate" + dob);
            }
            if(flexAccountDetails.ADDR2==null)
            {
                flexAccountDetails.ADDR2 = "PMD 43";
            }
            
            string address = flexAccountDetails.ADDR1 + flexAccountDetails.ADDR2;
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
            //-------End newly added  fields for NI migration----------------
            //rtn.ProductFields = printFields.Select(s => new ProductPrinting.ProductField(s)).ToList();
            return rtn;
        }
        private string DecodeAccType(string accountNum,string accountType)
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

        private int DecodeCurrency(string ccy)
        {
            return DataSource.LookupDAL.LookupCurrency(ccy);
        }

        private string encodeCurrency(int currencyId)
        {
            return DataSource.LookupDAL.LookupCurrency(currencyId);
        }

        private int DecodeAccountType(string acctype)
        {
            //0	CURRENT
            //1	SAVINGS
            //2	CHEQUE
            //3	CREDIT
            //4	UNIVERSAL
            //5	INVESTMENT

            switch (acctype.Trim().ToUpper())
            {
                case "C": return 0;
                case "S": return 1;
                default: throw new ArgumentException("Unknown Account Type.");
            }
        }

        private void DecodeName(string custname, out string firstName, out string middleName, out string lastName)
        {
            firstName = String.Empty;
            middleName = String.Empty;
            lastName = String.Empty;

            string[] splitName = custname.Trim().Split();

            lastName = splitName[splitName.Length - 1].Trim();

            for (int i = 0; i < splitName.Length - 1; i++)
            {
                if (!String.IsNullOrWhiteSpace(splitName[i]))
                {
                    if (String.IsNullOrWhiteSpace(firstName) && i == 0)
                        firstName = splitName[i];
                    else
                        middleName += splitName[i] + " ";
                }
                //if (splitName.Length > 1)
                //    firstName = splitName[0].Trim();

                //if (splitName.Length > 2)
                //{
                //    middleName = splitName[1].Trim();
                //}
            }

            firstName = firstName.Trim();
            middleName = middleName.Trim();
            lastName = lastName.Trim();
        }

        private string BuildHtmlMessage(List<Tuple<string, string>> messages)
        {
            StringBuilder msgBuilder = new StringBuilder();
            foreach (var message in messages)
            {
                msgBuilder.AppendFormat("{0}<br />", message.Item2);
            }

            return msgBuilder.ToString();
        }
        #endregion
    }
}
