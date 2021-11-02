using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.Fidelity.DAL;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.ProductPrinting;
using Veneka.Module.OracleFlexcube;
using Veneka.Module.OracleFlexcube.Utils;

namespace Veneka.Indigo.Integration.Fidelity.Flexcube
{
   public class MasterCardFlexcubeWebService
    {
        #region Private Fields
        private static readonly ILog _cbsLog = LogManager.GetLogger(Veneka.Indigo.Integration.Common.General.CBS_LOGGER);
        private static readonly ILog _atmupdateflagLog = LogManager.GetLogger(Veneka.Indigo.Integration.Common.General.ATMFLAGUPDATE_LOGGER);


        private readonly AccountServicesValidated _flexAccService;
        private readonly MelcomRTServicesValidated _flexMelcomRtService;
        private readonly CustomerServiceValidated _flexCustService;
        private readonly WebServiceConfig _ubsParms;
        private readonly WebServiceConfig _rtParms;

        //private readonly LookupDAL _lookupDAL;
        //private readonly TransactionSequenceDAL _sequenceDAL;
        public IDataSource DataSource { get; set; }
        private readonly string _connectionString;
        #endregion
        #region Constructors
        public MasterCardFlexcubeWebService(Config.WebServiceConfig parameters, IDataSource _dataSource)
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
            //this._connectionString = connectionString;
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
                                                 parameters.Timeout, Veneka.Module.OracleFlexcube.ServicesValidated.Authentication.NONE, parameters.Username, parameters.Password, parameters.Nonce, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), Veneka.Indigo.Integration.Common.General.CBS_LOGGER);

            MelcomRTServicesValidated.Protocol rtProtocol = MelcomRTServicesValidated.Protocol.HTTPS;

            switch (parameters.Protocol)
            {
                case Protocol.HTTP: rtProtocol = MelcomRTServicesValidated.Protocol.HTTP; break;
                case Protocol.HTTPS: rtProtocol = MelcomRTServicesValidated.Protocol.HTTPS; break;
                default: break;
            }

            //Initialise Flexcube RealTime Services
            //"/FCUBSRTService/FCUBSRTService"
            _flexMelcomRtService = new MelcomRTServicesValidated(rtProtocol, parameters.Address, parameters.Port, flexRtServicePath,
                                                 parameters.Timeout, Veneka.Module.OracleFlexcube.ServicesValidated.Authentication.NONE, parameters.Username, parameters.Password, parameters.Nonce, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), Veneka.Indigo.Integration.Common.General.CBS_LOGGER);

            CustomerServiceValidated.Protocol custProtocol = CustomerServiceValidated.Protocol.HTTPS;
            switch (parameters.Protocol)
            {
                case Protocol.HTTP: custProtocol = CustomerServiceValidated.Protocol.HTTP; break;
                case Protocol.HTTPS: custProtocol = CustomerServiceValidated.Protocol.HTTPS; break;
                default: break;
            }
            _flexCustService = new CustomerServiceValidated(custProtocol, parameters.Address, parameters.Port, flexCustServicePath,
                parameters.Timeout, Veneka.Module.OracleFlexcube.ServicesValidated.Authentication.NONE, parameters.Username, parameters.Password, parameters.Nonce, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), Veneka.Indigo.Integration.Common.General.CBS_LOGGER);


            // _lookupDAL = new LookupDAL(connectionString);

            // _sequenceDAL = new TransactionSequenceDAL(connectionString);
        }

        #endregion
        #region Public Methods
        /// <summary>
        /// Queries Flexcube for the desired customers account and balance
        /// </summary>
        /// <param name="acc"></param>
        public bool QueryCustAcc(string accountNumber, List<ProductPrinting.IProductPrintField> printFields, string branchCode, int languageId,string accNumPrefix, out AccountDetails accDetails, out string responseMessage)
        {
            _flexAccService.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            accDetails = null;
            Veneka.Module.OracleFlexcube.UBSAccWebService.CustAccountFullType custAcc;
            //Build unique indigo reference for this transaction
            string indigoReference = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');
            //Step 1: Find the customer account
            if (!_flexAccService.QueryCustomerAccount(accountNumber, branchCode, indigoReference, out custAcc, out messages))
            {
                _cbsLog.Debug("customer account lookup failed");
                //Check if the custAcc is null
                responseMessage = BuildHtmlMessage(messages);
                _cbsLog.Debug("responseMessage : " + responseMessage);
                return false;
            }
            else
            {
                _cbsLog.Debug("Feedback Message " + messages);
                if (custAcc == null)
                {
                    responseMessage = "Customer not found in CBS";
                    return false;
                }
                if (string.IsNullOrEmpty(custAcc.CUSTNAME))
                {
                    _cbsLog.Debug("customer account lookup not successful");
                    responseMessage = "Customer not found in CBS";
                    return false;
                }

            }

            // Step 2 :Look for customer DOB

            string customerNum = custAcc.CUSTNO;
            Veneka.Module.OracleFlexcube.UBSCustWebService.CustomerFullType custDetails;
            List<Tuple<string, string>> custDetailsmessages;
          

            if (!_flexCustService.QueryCustomerDetails(customerNum, branchCode, indigoReference, out custDetails, out custDetailsmessages))
            {
                //Check if the custDetails is null
                responseMessage = BuildHtmlMessage(custDetailsmessages);
                return false;
            }
            string mobileNumber = custDetails.Custpersonal.MOBNUM.ToString();
            if (mobileNumber == null)
            {
                responseMessage = "Mobile number is empty";
                return false;
            }

            accDetails = BuildAccountDetails(custAcc, printFields, custDetails, accNumPrefix);
            accDetails.AdditionalAccountNumber = !string.IsNullOrEmpty(accDetails.AccountNumber) && accDetails.AccountNumber.StartsWith(accNumPrefix) ? accountNumber : accDetails.AccountNumber;
            //accDetails.AdditionalAccountNumber = !string.IsNullOrEmpty(accDetails.AccountNumber) && accDetails.AccountNumber.StartsWith("MLCM_") ? accountNumber : accDetails.AccountNumber;


            _cbsLog.Debug("Integration -- is acc in CBS :  " + accDetails.IsCBSAccount);


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
            string indigoReference = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

            //Step 1: Check that account has enough funds            
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
        public bool ChargeFee(CustomerDetails customerDetails, string operatorUsername, string eCashAccount,string BranchFundsLoadAccount, int languageId,bool IsIssuance, out string responseMessage)
        {
            string strNarration = string.Empty;
            if(IsIssuance)
            {
                strNarration = "MC ISSUANCE FEE B/O";
            }
            else
            {
                strNarration = "MC PREPAID CARD LOAD B/O";
            }

            _flexMelcomRtService.LanguageId = languageId;
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
            string indigoReference1 = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference1 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference1.Length), '0');
            //Step 1: Check that account has enough funds
            string branch = customerDetails.IsCBSAccountHolder ? branchCode : "001";
            if (!_flexAccService.QueryAccountBalance(eCashAccount, branch, indigoReference1, out accBalance, out messages))
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
            string indigoReference = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

                                            
            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                                .ToString().PadLeft((16 - indigoReference.Length), '0');

            customerDetails.FeeReferenceNumber = indigoReference;

            string MessageId = String.Format("MCM{0}{1}", DateTime.Now.ToString("yy"),
                                                                   DateTime.Now.DayOfYear);

            MessageId += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - MessageId.Length), '0');
            if (indigoReference.Length > 16)
            {
                responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigoReference);
                return false;
            }

            string flexcubeReference;
           
            if (!_flexMelcomRtService.CreateDebitTransactionFS(IsIssuance ? Module.OracleFlexcube.Utils.General.TransactionCode.MITD : Module.OracleFlexcube.Utils.General.TransactionCode.MITC,
                operatorUsername,customerDetails.AccountNumber,customerDetails.NameOnCard,customerDetails.CardNumber, customerDetails.CardReference, eCashAccount, branch, branch, indigoReference, strNarration, ccy, customerDetails.FeeCharge.Value, MessageId, out flexcubeReference, out messages))
            {
                responseMessage = BuildHtmlMessage(messages);
                return false;
            }

            if (flexcubeReference==null)
            {
                responseMessage= String.Format("Failed to debit eCash account");
                return false;
            }
            //Build unique indigo reference for this transaction
            string indigocreditReference = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigocreditReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

            customerDetails.FeeReferenceNumber = indigoReference;

            string MessageCreditId = String.Format("MCM{0}{1}", DateTime.Now.ToString("yy"),
                                                                   DateTime.Now.DayOfYear);

            MessageCreditId += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - MessageId.Length), '0');
            if (indigocreditReference.Length > 16)
            {
                responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigocreditReference);
                return false;
            }



            if(!IsIssuance)//only do this for funding 
            {
                #region 2nd leg taken care of in debit transaction above
                _cbsLog.Debug(string.Format("Before calling CreateCreditTransactionFS(GITF)-- > Branch Funds-Load Account {0}", BranchFundsLoadAccount));
                if (!_flexMelcomRtService.CreateCreditTransactionFS(Module.OracleFlexcube.Utils.General.TransactionCode.MITF, customerDetails.NameOnCard, customerDetails.CardNumber, BranchFundsLoadAccount, customerDetails.CardReference, branch, branch, indigocreditReference, strNarration, ccy, customerDetails.FeeCharge.Value, MessageCreditId, out flexcubeReference, out messages))
                {
                    responseMessage = BuildHtmlMessage(messages);
                    return false;
                }
                if (flexcubeReference == null)
                {
                    responseMessage = String.Format("Failed to credit Fidelity income account");
                    return false;
                }
                //customerDetails.FeeReferenceNumber = flexcubeReference;
                _cbsLog.Debug(" customerDetails.FeeReferenceNumber==" + customerDetails.FeeReferenceNumber);
                #endregion
            }

            responseMessage = String.Empty;
            return true;
        }

        public bool ChargeFeeAtFidelityBank(CustomerDetails customerDetails, string tellerId, string tellerAccount, string headOfficeFundsLoadAccount, string branchFundsLoadAccount, int languageId, bool IsIssuance, out string responseMessage)
        {
            string strNarration = string.Empty;
            if (IsIssuance)
            {
                strNarration = "MasterCard ISSUANCE FEE B/O";
            }
            else
            {
                strNarration = "MasterCard PREPAID CARD LOAD B/O";
            }
            _flexMelcomRtService.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            // LookupDAL lDal = new LookupDAL(_connectionString);
            string branchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.DomicileBranchId);
            string chargeBranchCode = branchCode;
            if (customerDetails.ChargeFeeToIssuingBranch)
                chargeBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);

            string ccy = encodeCurrency(customerDetails.CurrencyId.Value);

            Veneka.Module.OracleFlexcube.UBSAccWebService.AccBalRestypeACC_BAL accBalance;
            _cbsLog.Debug("Calling Charge fee method in FlexcubeWebService class");
            //determine if its fidelity customer
            #region checking debit account balance(fidelity customer account or teller account)
            //Build unique indigo reference for this transaction
            string indigoReference1 = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference1 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference1.Length), '0');
            if (customerDetails.IsCBSAccountHolder)
            {
                //Step 1: Check that account has enough funds            
                //if (!_flexAccService.QueryAccountBalance(customerDetails.AccountNumber, branchCode, out accBalance, out messages))
                if (!_flexAccService.QueryAccountBalance(IsIssuance ? customerDetails.AdditionalAccountNumber:customerDetails.AccountNumber, branchCode, indigoReference1, out accBalance, out messages))
                {
                    responseMessage = BuildHtmlMessage(messages);
                    return false;
                }
                _cbsLog.Debug("Fidelity account holder -- check in cbs if it has enough balance");
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

                _cbsLog.Debug("Done with account balance");
            }
            else
            {
                _cbsLog.Debug("Non-Fidelity customer ");
            }

            #endregion
            //_cbsLog.Debug("Done with account balance and is greater than the chargefee");
            #region debit account (customer or non-fidelity account holder)
            //Build unique indigo reference for this transaction
            string indigoReference = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                    DateTime.Now.DayOfYear);

            indigoReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - indigoReference.Length), '0');

            customerDetails.FeeReferenceNumber = indigoReference;

            string MessageId = String.Format("MCM{0}{1}", DateTime.Now.ToString("yy"),
                                                                   DateTime.Now.DayOfYear);

            MessageId += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                                            .ToString().PadLeft((16 - MessageId.Length), '0');
            if (indigoReference.Length > 16)
            {
                responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigoReference);
                return false;
            }

            string flexcubeReference;
            //if fidelit
            string accountToDebit = customerDetails.IsCBSAccountHolder ? customerDetails.AccountNumber : tellerAccount;
            string currBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);
            string branchOfAccToDebit = string.Empty;
            string BRN = string.Empty;
            string TXNBRN = string.Empty;
            Module.OracleFlexcube.Utils.General.TransactionCode transactionCode;
            string lookUpAccountNo = string.Empty;
            string cardNumber = string.Empty;

            if (IsIssuance)
            {
                
                 branchOfAccToDebit = customerDetails.IsCBSAccountHolder ? branchCode : currBranchCode;
                 accountToDebit = customerDetails.IsCBSAccountHolder ? customerDetails.AdditionalAccountNumber : tellerAccount;
                 BRN = currBranchCode;
                 TXNBRN = customerDetails.IsCBSAccountHolder ? branchCode : currBranchCode;
                 transactionCode = Module.OracleFlexcube.Utils.General.TransactionCode.MITD;
                 lookUpAccountNo = customerDetails.AdditionalAccountNumber;
                 cardNumber = DataSource.CardsDAL.FetchPanByRefNumber(customerDetails.CardNumber, 1, string.Empty);
                _cbsLog.Debug($"Fetched Card {cardNumber.Substring(0, 4) + "****" + cardNumber.Substring(cardNumber.Length - 4, 4) }, with ref {customerDetails.CardNumber}");

            }
            else
            {
                branchOfAccToDebit = customerDetails.IsCBSAccountHolder ? branchCode : customerDetails.FundingDetails.BranchCode;
                //branchOfAccToDebit = currBranchCode;// customerDetails.FundingDetails.BranchCode;
                BRN = currBranchCode;
                TXNBRN = customerDetails.IsCBSAccountHolder ? branchCode : currBranchCode;
                transactionCode = Module.OracleFlexcube.Utils.General.TransactionCode.MITC;
                lookUpAccountNo = customerDetails.AccountNumber;
                cardNumber = customerDetails.CardNumber;
            }
            //string branchOfAccToDebit = customerDetails.IsCBSAccountHolder ? branchCode : customerDetails.FundingDetails.BranchCode;
            //if (!_flexMelcomRtService.CreateDebitTransactionFS(accountToDebit, branchOfAccToDebit, branchOfAccToDebit, indigoReference, ccy, customerDetails.FeeCharge.Value, MessageId, out flexcubeReference, out messages))

            if (customerDetails.IsCBSAccountHolder)
            {
                _cbsLog.Debug("debit customer account ");
                //_flexMelcomRtService.CreateDebitTransactionFS(customerDetails.NameOnCard, customerDetails.CardNumber, eCashAccount, "001", "001", indigoReference, ccy, customerDetails.FeeCharge.Value, MessageId, out flexcubeReference, out messages)
                if (!_flexMelcomRtService.CreateDebitTransactionFS(transactionCode,tellerId, lookUpAccountNo, customerDetails.NameOnCard, cardNumber, customerDetails.CardReference, accountToDebit, BRN, TXNBRN, indigoReference, strNarration, ccy, customerDetails.FeeCharge.Value, MessageId, out flexcubeReference, out messages))
                {
                    responseMessage = BuildHtmlMessage(messages);
                    return false;
                }
            }
            else
            {
                _cbsLog.Debug("debit teller account ");
                //string tellerId = customerDetails.FundingDetails.TellerId;

                string denominations = string.Empty;
                foreach (AmountDenomination denom in customerDetails.FundingDetails.LoadDenominations)
                {
                    if(denom.DenomUnits > 0)
                    denominations += string.Format("{0}:{1}:{2}:{3}|",denom.DenomCode,denom.DenomCCY,denom.DenomValue,denom.DenomUnits);
                }
                _cbsLog.Debug(string.Format("denominations to load {0}", denominations));
                if (!_flexMelcomRtService.CreateDebitTransactionFS(transactionCode,tellerId, denominations, customerDetails.NameOnCard, customerDetails.CardNumber, customerDetails.CardReference, accountToDebit, branchOfAccToDebit, branchOfAccToDebit, indigoReference, strNarration, ccy, customerDetails.FeeCharge.Value, MessageId, out flexcubeReference, out messages))
                {
                    responseMessage = BuildHtmlMessage(messages);
                    return false;
                }
            }

            if (flexcubeReference == null)
            {
                responseMessage = String.Format("Failed to debit customer or teller account");
                return false;
            }
            #endregion

            if(!IsIssuance)//only do this for funding
            {
                #region credit branch account -- taken out
                ////Build unique indigo reference for this transaction
                //string indigocreditReference = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                //                                                        DateTime.Now.DayOfYear);

                //indigocreditReference += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                //                                .ToString().PadLeft((16 - indigocreditReference.Length), '0');

                //customerDetails.FeeReferenceNumber = indigocreditReference;

                //string MessageCreditId = String.Format("MCM{0}{1}", DateTime.Now.ToString("yy"),
                //                                                       DateTime.Now.DayOfYear);

                //MessageCreditId += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                //                                .ToString().PadLeft((16 - MessageId.Length), '0');
                //if (indigocreditReference.Length > 16)
                //{
                //    responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigocreditReference);
                //    return false;
                //}

                ////credit branch account
                //chargeBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);
                //_cbsLog.Debug("credit branch account ");
                ////if (!_flexMelcomRtService.CreateCreditTransactionFS(branchFundsLoadAccount, chargeBranchCode, chargeBranchCode, indigocreditReference, ccy, customerDetails.FeeCharge.Value, MessageCreditId, out flexcubeReference, out messages))
                //if (!_flexMelcomRtService.CreateCreditTransactionFS(customerDetails.NameOnCard, customerDetails.CardNumber, branchFundsLoadAccount, chargeBranchCode, chargeBranchCode, indigocreditReference, strNarration, ccy, customerDetails.FeeCharge.Value, MessageCreditId, out flexcubeReference, out messages))
                //{
                //    responseMessage = BuildHtmlMessage(messages);
                //    return false;
                //}
                //if (flexcubeReference == null)
                //{
                //    responseMessage = String.Format("Failed to credit branch account");
                //    return false;
                //}
                //customerDetails.FeeReferenceNumber = flexcubeReference;
                //_cbsLog.Debug(" customerDetails.FeeReferenceNumber==" + customerDetails.FeeReferenceNumber);
                #endregion

                #region debit branch account
                //debit branch account

                string indigoReference2 = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                                                                        DateTime.Now.DayOfYear);

                indigoReference2 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                                                .ToString().PadLeft((16 - indigoReference2.Length), '0');

                customerDetails.FeeReferenceNumber = indigoReference2;

                string MessageId2 = String.Format("MCM{0}{1}", DateTime.Now.ToString("yy"),
                                                                       DateTime.Now.DayOfYear);

                MessageId2 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                                                .ToString().PadLeft((16 - MessageId2.Length), '0');
                if (indigoReference2.Length > 16)
                {
                    responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigoReference);
                    return false;
                }
                _cbsLog.Debug("debit branch account ");

                string flexcubeReference2;

                BRN = currBranchCode;
                TXNBRN = currBranchCode;

                //
                //if (!_flexMelcomRtService.CreateDebitTransactionFS(Module.OracleFlexcube.Utils.General.TransactionCode.GITF, customerDetails.NameOnCard, customerDetails.CardNumber, branchFundsLoadAccount, chargeBranchCode, chargeBranchCode, indigoReference2, strNarration, ccy, customerDetails.FeeCharge.Value, MessageId2, out flexcubeReference2, out messages))
                if (!_flexMelcomRtService.CreateDebitTransactionFS(Module.OracleFlexcube.Utils.General.TransactionCode.MITF,tellerId,customerDetails.AccountNumber,customerDetails.NameOnCard, customerDetails.CardNumber, customerDetails.CardReference, branchFundsLoadAccount, BRN, TXNBRN, indigoReference2, strNarration, ccy, customerDetails.FeeCharge.Value, MessageId2, out flexcubeReference2, out messages))
                {
                    responseMessage = BuildHtmlMessage(messages);
                    return false;
                }

                if (flexcubeReference2 == null)
                {
                    responseMessage = String.Format("Failed to debit branch account");
                    return false;
                }

                #endregion

                //credit fidelity income account
                #region credit head office account -- this has been taken out


                ////Build unique indigo reference for this transaction
                //string indigocreditReference2 = String.Format("MC{0}{1}", DateTime.Now.ToString("yy"),
                //                                                        DateTime.Now.DayOfYear);

                //indigocreditReference2 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubetxn", ResetPeriod.DAILY)
                //                                .ToString().PadLeft((16 - indigocreditReference2.Length), '0');

                //customerDetails.FeeReferenceNumber = indigocreditReference2;

                //string MessageCreditId3 = String.Format("MCM{0}{1}", DateTime.Now.ToString("yy"),
                //                                                       DateTime.Now.DayOfYear);

                //MessageCreditId3 += DataSource.TransactionSequenceDAL.NextSequenceNumber("flexcubechargefeetxn", ResetPeriod.DAILY)
                //                                .ToString().PadLeft((16 - MessageId.Length), '0');
                //if (indigocreditReference2.Length > 16)
                //{
                //    responseMessage = String.Format("Internal Indigo Reference is to long, must be 16 characters {0}", indigocreditReference2);
                //    return false;
                //}

                ////credit head office account
                //string flexcubeReference3;

                //if (IsIssuance)
                //{
                //    _cbsLog.Debug("credit headoffice account for issuance");
                //    chargeBranchCode = DataSource.LookupDAL.LookupBranchCode(customerDetails.BranchId);
                //    if (!_flexMelcomRtService.CreateCreditTransactionFS(customerDetails.NameOnCard, customerDetails.CardNumber, headOfficeFundsLoadAccount, chargeBranchCode, chargeBranchCode, indigocreditReference2, strNarration, ccy, customerDetails.FeeCharge.Value, MessageCreditId3, out flexcubeReference3, out messages))
                //    {
                //        responseMessage = BuildHtmlMessage(messages);
                //        return false;
                //    }

                //}
                //else
                //{
                //    _cbsLog.Debug("credit headoffice account for fundsload");
                //    if (!_flexMelcomRtService.CreateCreditTransactionFS(customerDetails.NameOnCard, customerDetails.CardNumber, headOfficeFundsLoadAccount, "000", "000", indigocreditReference2, strNarration, ccy, customerDetails.FeeCharge.Value, MessageCreditId3, out flexcubeReference3, out messages))
                //    {
                //        responseMessage = BuildHtmlMessage(messages);
                //        return false;
                //    }
                //}

                //if (flexcubeReference3 == null)
                //{
                //    responseMessage = String.Format("Failed to credit headOffice account");
                //    return false;
                //}
                //customerDetails.FeeReferenceNumber = flexcubeReference3;
                //_cbsLog.Debug(" customerDetails.FeeReferenceNumber==" + customerDetails.FeeReferenceNumber);

                #endregion

            }



            responseMessage = String.Empty;
            return true;
        }

        public bool ReverseFee(string accountNumber, string branchCode, string ccy, decimal transactionAmount, int languageId, out string responseMessage)
        {
            _flexMelcomRtService.LanguageId = languageId;
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
            if (_flexAccService.QueryCustomerAccount(customerDetails.AccountNumber, branchCode,"", out custAccount, out messages))
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

        private AccountDetails BuildAccountDetails(Veneka.Module.OracleFlexcube.UBSAccWebService.CustAccountFullType flexAccountDetails, List<ProductPrinting.IProductPrintField> printFields, Veneka.Module.OracleFlexcube.UBSCustWebService.CustomerFullType custDetails, string AccNumPrefix)
        {
            string firstName, middlename, lastname;

            DecodeName(flexAccountDetails.CUSTNAME, out firstName, out middlename, out lastname);

            AccountDetails rtn = new AccountDetails
            {
                 AccountNumber = string.Format("{0}_{1}",AccNumPrefix, custDetails.Custpersonal.MOBNUM.ToString()),
                //AccountTypeId =-1, // DecodeAccountType(flexAccountDetails.ACCTYPE),
                CurrencyId = DecodeCurrency(flexAccountDetails.CCY),
                //CBSAccountTypeId=flexAccountDetails.ACCTYPE,
                CBSAccountTypeId = DecodeAccType(flexAccountDetails.ACC, flexAccountDetails.ACCTYPE),
                FirstName = firstName,
                LastName = lastname,
                MiddleName = middlename,
                Address1 = flexAccountDetails.ADDR1,
                Address2 = flexAccountDetails.ADDR2,
                Address3 = flexAccountDetails.ADDR3,
                ContactNumber = custDetails.Custpersonal.MOBNUM.ToString(),
                IsCBSAccount = true
            };
            _cbsLog.Debug("CBSAccountTypeId==" + rtn.CBSAccountTypeId);
            //-------Start newly added  fields for NI migration----------------
            string dob = DateTime.Now.ToString();
            if (custDetails.Custpersonal.DOB != null)
            {
               
                dob = custDetails.Custpersonal.DOB.ToString();
                _cbsLog.Debug("Date of birth" + dob);
            }
            else
            if (custDetails.Custcorp.INCORPDT != null)
            {
                dob = custDetails.Custcorp.INCORPDT.ToString();
                _cbsLog.Debug("Date incorporate" + dob);
            }
            if (flexAccountDetails.ADDR2 == null)
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
