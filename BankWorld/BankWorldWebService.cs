using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.Fidelity.Objects;
using System.ServiceModel;
using System.Xml;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.ServiceModel.Channels;
using Veneka.Module.CR2BankWorld;
using Common.Logging;
using Veneka.Indigo.Integration.DAL;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.Fidelity.DAL;
using System.Globalization;

namespace Veneka.Indigo.Integration.Fidelity.BankWorld
{
    public class BankWorldWebService
    {
        private static readonly ILog _cbsLog = LogManager.GetLogger(General.CBS_LOGGER);

        private readonly CardServicesValidated _cardServices;
        private readonly WebServiceConfig _parms;
        // private readonly LookupDAL _lookupDAL;
        public IDataSource DataSource { get; set; }
        public BankWorldWebService(string addesss, string path, int port, string username, string password, IDataSource _datasource)
        {
           
            DataSource = _datasource;
            Veneka.Module.CR2BankWorld.Utils.General.Protocol protocol = Module.CR2BankWorld.Utils.General.Protocol.HTTP;

            //switch (paramters.Protocol)
            //{
            //    case Protocol.HTTP: protocol = Module.CR2BankWorld.Utils.General.Protocol.HTTP; break;
            //    case Protocol.HTTPS: protocol = Module.CR2BankWorld.Utils.General.Protocol.HTTPS; break;
            //    default: break;
            //}

            _cardServices = new CardServicesValidated(protocol, addesss, port, path,
                                                      null, Veneka.Module.CR2BankWorld.Utils.General.Authentication.NONE, username, password, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), General.CMS_LOGGER);

        }

        public BankWorldWebService(WebServiceConfig paramters, IDataSource _datasource)
        {
            this._parms = paramters;
            DataSource = _datasource;
            Veneka.Module.CR2BankWorld.Utils.General.Protocol protocol = Module.CR2BankWorld.Utils.General.Protocol.HTTP;

            switch (paramters.Protocol)
            {
                case Protocol.HTTP: protocol = Module.CR2BankWorld.Utils.General.Protocol.HTTP; break;
                case Protocol.HTTPS: protocol = Module.CR2BankWorld.Utils.General.Protocol.HTTPS; break;
                default: break;
            }

            _cardServices = new CardServicesValidated(protocol, paramters.Address,paramters.Port, paramters.Path,
                                                      paramters.Timeout, Veneka.Module.CR2BankWorld.Utils.General.Authentication.NONE, paramters.Username,paramters.Password, new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), General.CMS_LOGGER);

            //_lookupDAL = new LookupDAL(connectionString);
        }

        /// <summary>
        /// Update the card holder details and the accounts linked to a card
        /// </summary>        
        public bool UpdateCard(CustomerDetails customerDetails, int languageId, out string responseMessage)
        {
            _cardServices.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            //TODO: Fixup some of these datapoints
            Module.CR2BankWorld.BWCardsServices.AccountInformation accInfo = new Module.CR2BankWorld.BWCardsServices.AccountInformation
            {
                AccountNumber = customerDetails.AccountNumber.Trim(),
                AccountSystemID = "1",
                Account_Type = EncodeAccountType(customerDetails.AccountTypeId.Value),
                Currency_Code = DataSource.LookupDAL.LookupCurrencyISONumericCode(customerDetails.CurrencyId.Value),// "936", //customerDetails.CurrencyId.ToString(),
                Account_IDX = "0",
                SR_Bal_Enq = 'Y',
                SR_Bill_Payment = 'Y',
                SR_CCY_Cash = 'Y',
                SR_CTOC_Transfer = 'Y',
                SR_Cash = 'Y',
                SR_Cash_Back = 'Y',
                SR_Change_PIN = 'Y',
                SR_Cheq_BK_Req = 'Y',
                SR_Deposit = 'Y',
                SR_Draft_Req = 'Y',
                SR_FULL_STMT = 'Y',
                SR_MINI_STMT = 'Y',
                SR_Manual_Cash = 'Y',
                SR_Notic_WDL = 'Y',
                SR_Quasi_Cash = 'Y',
                SR_STMT_Req = 'Y',
                SR_Sale = 'Y',
                SR_Transfer_From = 'Y',
                SR_Transfer_To = 'Y',
                SR_Trav_Cheq = 'Y'
            };

            Module.CR2BankWorld.BWCardsServices.CardObj cardObj = new Module.CR2BankWorld.BWCardsServices.CardObj
            {
                CardNo = customerDetails.CardNumber.Trim(),
                CardStatus = "NORMAL",
                CardHolderTitle = TitleLookup(customerDetails.CustomerTitleId),
                CardholderFirstName = customerDetails.FirstName.Trim(),
                CardholderLastName = customerDetails.LastName.Trim(),
                SequenceNumber = "0",
                AccountInformations = new Module.CR2BankWorld.BWCardsServices.AccountInformation[]{
                    accInfo
                }
            };
         
            if (!_cardServices.UpdateCard(cardObj, out messages))
            {
                StringBuilder msgBuilder = new StringBuilder();
                foreach (var message in messages)
                {
                    msgBuilder.AppendFormat("{0}<br />", message.Item2);
                }

                responseMessage = msgBuilder.ToString();
                return false;
            }

            responseMessage = "";
            return true;
        }

        public bool CreateCard(CustomerDetails customerDetails, DateTime? expirydate, string cardType,string cashLimit, string address, string serviceCode,int languageId, out string responseMessage)
        {
            string acctype = string.Empty;
            string CC = DataSource.LookupDAL.LookupCurrencyISONumericCode(customerDetails.CurrencyId.Value);
            if (customerDetails.AccountNumber.Trim().StartsWith("1"))
            {
                if (CC == "936")
                {
                    acctype = "20";
                }
            }
            if (CC == "840")
            {
                acctype = "25";
            }
            if (CC == "978")
            {
                acctype = "28";
            }
            if (CC == "826")
            {
                acctype = "30";
            }
            //for savings
            if (customerDetails.AccountNumber.Trim().StartsWith("2"))
            {
                if (CC == "936")
                {
                    acctype = "10";
                }
            }
            if (CC == "840")
            {
                acctype = "26";
            }
            if (CC == "978")
            {
                acctype = "27";
            }
            if (CC == "826")
            {
                acctype = "29";
            }

            _cbsLog.Debug("Currency code the BW" + CC);

            _cbsLog.Debug("Account Type to the BW" + acctype);

            _cardServices.LanguageId = languageId;
            List<Tuple<string, string>> messages;
            
            //TODO: Fixup some of these datapoints
            Module.CR2BankWorld.BWCardsServices.AccountInformation accInfo = new Module.CR2BankWorld.BWCardsServices.AccountInformation
            {
                AccountNumber = customerDetails.AccountNumber.Trim(),
                AccountSystemID = "1",
                Account_Type = acctype,
                Currency_Code = CC,// "936", //customerDetails.CurrencyId.ToString(),
                Account_IDX = "0",
                SR_Bal_Enq = 'Y',
                SR_Bill_Payment = 'Y',
                SR_CCY_Cash = 'Y',
                SR_CTOC_Transfer = 'Y',
                SR_Cash = 'Y',
                SR_Cash_Back = 'Y',
                SR_Change_PIN = 'Y',
                SR_Cheq_BK_Req = 'Y',
                SR_Deposit = 'Y',
                SR_Draft_Req = 'Y',
                SR_FULL_STMT = 'Y',
                SR_MINI_STMT = 'Y',
                SR_Manual_Cash = 'Y',
                SR_Notic_WDL = 'Y',
                SR_Quasi_Cash = 'Y',
                SR_STMT_Req = 'Y',
                SR_Sale = 'Y',
                SR_Transfer_From = 'Y',
                SR_Transfer_To = 'Y',
                SR_Trav_Cheq = 'Y'
                
            };

            Module.CR2BankWorld.BWCardsServices.CardObj cardObj = new Module.CR2BankWorld.BWCardsServices.CardObj
            {
                CardNo =customerDetails.CardNumber.Trim(),
                ExpiryDate = expirydate?.ToString ("yyyyMMdd"),
                CardStatus = "NORMAL",
                CardHolderTitle = TitleLookup(customerDetails.CustomerTitleId),
                CardholderFirstName = customerDetails.FirstName.Trim(),
                CardholderLastName = customerDetails.LastName.Trim(),
                SequenceNumber = "0",
                Branchcode = customerDetails.BranchCode,
                AddressLine1=address,
                AddressLine2=address,
                AddressLine3=address,
                AddressLine4=address,
                MobilePhoneNumber=customerDetails.ContactNumber,
                Language="en",                
                CardType =cardType,
                CashLimit= cashLimit,
                ServiceCode =serviceCode,
                AccountInformations = new Module.CR2BankWorld.BWCardsServices.AccountInformation[]{
                    accInfo
                }
            };

            if (!_cardServices.CreateCard(cardObj, out messages))
            {
                StringBuilder msgBuilder = new StringBuilder();
                foreach (var message in messages)
                {
                    msgBuilder.AppendFormat("{0}<br />", message.Item2);
                }

                responseMessage = msgBuilder.ToString();
                return false;
            }

            responseMessage = "";
            return true;
        }
        /// <summary>
        /// Update the PIN
        /// </summary>    
        public bool UpdatePVV(string cardNo, string PVVOffset, int languageId, out string responseMessage)
        {
            _cardServices.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            Module.CR2BankWorld.BWCardsServices.GeneratedCardObj genCardObj = new Module.CR2BankWorld.BWCardsServices.GeneratedCardObj
            {
                CardNo = cardNo.Trim(),
                PVV_OFFSET = PVVOffset.Trim()
            };

            if (!_cardServices.UpdatePVV(genCardObj, out messages))
            {
                StringBuilder msgBuilder = new StringBuilder();
                foreach (var message in messages)
                {
                    msgBuilder.AppendFormat("{0}<br />", message.Item2);
                }

                responseMessage = msgBuilder.ToString();
                return false;
            }

            responseMessage = "";
            return true;
        }

        /// <summary>
        /// Activate the card
        /// </summary>    
        public bool ActivateCard(string cardNo, string status, int languageId, out string responseMessage)
        {
            _cardServices.LanguageId = languageId;
            List<Tuple<string, string>> messages;

            if (!_cardServices.UpdateCardStatus(cardNo.Trim(), status.Trim(), out messages))
            {
                StringBuilder msgBuilder = new StringBuilder();
                foreach (var message in messages)
                {
                    msgBuilder.AppendFormat("{0}<br />", message.Item2);
                }

                responseMessage = msgBuilder.ToString();
                return false;
            }

            responseMessage = "";
            return true;
        }

        private string TitleLookup(int titleId)
        {
            switch (titleId)
            {
                case 0: return "MR";
                case 1: return "MRS";
                case 2: return "MISS";
                case 3: return "MS";
                case 4: return "PROF";
                case 5: return "DR";
                case 6: return "REV";
                case 7: return "OTHER";
                default: return "";
            }
        }

        //20 - current, 
        //10 - savings
        //20 - cheque
        private string EncodeAccountType(int accountTypeId)
        {
            switch (accountTypeId)
            {
                case 0: return "20";
                case 1: return "10";
                case 2: return "20";
                default: throw new ArgumentException("Unknown Account Type.");
            }
        }
    }
}