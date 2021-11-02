using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Veneka.Indigo.Integration;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Fidelity.Objects;
using System.Xml;
using System.IO;
using Common.Logging;
using System.ComponentModel.Composition;
using Veneka.Indigo.Integration.Fidelity.BankWorld;
using Veneka.Indigo.Integration.DAL;
using Veneka.Indigo.Integration.Fidelity.Flexcube;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.External;
using Veneka.Indigo.Integration.Fidelity.BranchFile.Objects;
using Veneka.Indigo.Integration.Fidelity.Util;
using Veneka.Indigo.Integration.Fidelity.BranchFile;
using Veneka.Indigo.Integration.Remote;


namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityCR2CMS", "9B424A7D-D2C9-4D4E-8317-870C8889D7CF", typeof(ICardManagementSystem))]
    public class FidelityCR2CMS : ICardManagementSystem
    {
        private static readonly ILog _cmsLog = LogManager.GetLogger(General.CMS_LOGGER);
        //private CardGeneratorDAL cardGeneratorDAL;

        public event EventHandler<DistEventArgs> OnUploadCompleted;

        public string SQLConnectionString { get; set; }

        public bool OnUploadCompletedSubscribed { get; set; }

        public System.IO.DirectoryInfo IntegrationFolder { get; set; }
        public IDataSource DataSource { get; set; }

        public bool AccountLookup(int issuerId, int productId, int cardIssueReasonId, string accountNumber, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, ref AccountDetails accountDetails, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
        }

        public LinkResponse LinkCardToAccountAndActive(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            ///-------------------------------FOR MIGRATION-----------------------------------------------
            //    responseMessage = String.Empty;

            //    try
            //    {
            //        WebServiceConfig webConfig = null;
            //        if (config is WebServiceConfig)
            //            webConfig = (WebServiceConfig)config;
            //        else
            //            throw new ArgumentException("Config parameters must be for Webservice.");

            //        //split address field for bankworld and NI cms ;
            //        string fullAddress = webConfig.Address;
            //        _cmsLog.Debug("fullAddress =" + fullAddress);
            //        string[] details = fullAddress.Split(';');
            //        _cmsLog.Debug("details =" + details);
            //        string bankworldAddress = string.Empty;
            //        string niAddress = string.Empty;
            //        //string niPort = string.Empty;
            //        if (details == null)
            //        {
            //            responseMessage = "Address field is not conigured for Bankworld or NI";
            //        }
            //        else
            //        {
            //            if (details.Length == 2)
            //            {
            //                bankworldAddress = details[0];
            //                niAddress = details[1];
            //                // niPort = details[2];
            //                _cmsLog.Debug("bankworldAddress =" + bankworldAddress);
            //                _cmsLog.Debug("niAddress =" + niAddress);
            //                //_cmsLog.Debug("restPath =" + niPort);
            //            }
            //            else
            //            {
            //                responseMessage = "bankworld Address;NIAddress are not configured correctly for CMS";
            //                throw new ArgumentException("bankworld Address;NIAddress are not configured correctly for CMS");
            //            }
            //        }

            //        string niUsername = webConfig.RemoteUsername;
            //        string niPassword = webConfig.RemotePassword;
            //        int niPort = Convert.ToInt16(webConfig.RemotePort);


            //        FIMIWebService fimiService = new FIMIWebService(niAddress, "", niPort, niUsername, niPassword, new DefaultDataDAL(this.DataSource));
            //        BankWorldWebService bankWorldService = new BankWorldWebService(bankworldAddress, webConfig.Path, webConfig.Port, webConfig.Username, webConfig.Password, DataSource);

            //        string fullname = customerDetails.FirstName;

            //        if (!String.IsNullOrWhiteSpace(customerDetails.MiddleName))
            //            fullname += " " + customerDetails.MiddleName;

            //        if (!String.IsNullOrWhiteSpace(customerDetails.LastName))
            //            fullname += " " + customerDetails.LastName;

            //        string branchCode = DataSource.LookupDAL.LookupEmpBranchCode(customerDetails.BranchId);

            //        //string branchCode = branchCodes[customerDetails.BranchId];

            //        if (String.IsNullOrWhiteSpace(branchCode))
            //            throw new ArgumentNullException("EMPBranchCode", "Branch does not have an EMP branch code specified.");


            //        DateTime dob = DateTime.Now;
            //        string address = String.Empty;
            //        string postCode = String.Empty;

            //        if (customerDetails.ProductFields != null && customerDetails.ProductFields.Count > 0)
            //        {
            //            foreach (var field in customerDetails.ProductFields)
            //            {
            //                if (field.MappedName.Equals("ind_sys_dob", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    //string dateofBirth = Encoding.ASCII.GetString(field.Value);
            //                    //dob = DateTime.ParseExact(dateofBirth, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            //                    dob = DateTime.Parse(Encoding.UTF8.GetString(field.Value), CultureInfo.InvariantCulture);
            //                    _cmsLog.Debug("EMPDOB field" + dob);
            //                }

            //                if (field.MappedName.Equals("ind_sys_address", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    address = Encoding.UTF8.GetString(field.Value);
            //                    _cmsLog.Debug("EMP ADDRESS " + address);
            //                }

            //                if (field.MappedName.Equals("ind_sys_postcode", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    postCode = Encoding.UTF8.GetString(field.Value);
            //                    _cmsLog.Debug("EMP Postal Code" + postCode);
            //                }
            //            }
            //        }

            //        // Comment field is built up by emp product account type and currency, and branch code
            //        // Each product's currency must have the emp_account_type variable set.

            //        string empAccountType = String.Empty;
            //        if (!customerDetails.CurrencyFields.TryGetValue("emp_account_type", out empAccountType))
            //        {
            //            responseMessage = String.Format("emp_account_type not set for product currency {0}. Please check product currency configuration.", customerDetails.CurrencyCode);
            //            return LinkResponse.RETRY;
            //        }

            //        string comment = String.Format("{0}~{1}", empAccountType, branchCode);

            //        //string comment = "~";
            //        //if (externalFields.Field != null)
            //        //    externalFields.Field.TryGetValue("comment", out comment);
            //        int cms_account_type;
            //        if (!string.IsNullOrEmpty(customerDetails.CMSAccountType))
            //        {
            //            int.TryParse(customerDetails.CMSAccountType, out cms_account_type);
            //        }
            //        else
            //        {
            //            throw new Exception("CMSAccountType is null");
            //        }

            //        var item = externalFields.Field.FirstOrDefault(i => i.Key == "cardType");
            //        if (item.Key == null)
            //        {
            //            throw new Exception("cardType external field not found.");
            //        }
            //        item = externalFields.Field.FirstOrDefault(i => i.Key == "cashLimit");
            //        if (item.Key == null)
            //        {
            //            throw new Exception("cashLimit external field not found.");
            //        }
            //        item = externalFields.Field.FirstOrDefault(i => i.Key == "serviceCode");
            //        if (item.Key == null)
            //        {
            //            throw new Exception("serviceCode external field not found.");
            //        }

            //        CardDetail card = new CardDetail();

            //        string cardType = externalFields.Field["cardType"].ToString();
            //        string cashLimit = externalFields.Field["cashLimit"].ToString();
            //        string serviceCode = externalFields.Field["serviceCode"].ToString();
            //        //added below to get the pan to update bankworld for Fidelity
            //        if (fimiService.FetchPan(int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, customerDetails.CustomerIDNumber, out string PAN, out DateTime? expiryDate, out responseMessage))
            //            customerDetails.CardNumber = PAN;
            //        _cmsLog.Debug("Log 1 PAN =" + PAN + "for customer=" + customerDetails.CardReference);
            //        card.card_expiry_date = expiryDate;
            //        _cmsLog.Debug("Log 1 expiryDate " + expiryDate);
            //        // Added till here for Fidelity to update bankworld

            //        if (fimiService.LinkCardToAccountAndActive(customerDetails.CardId, int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, fullname, customerDetails.AccountNumber,
            //       cms_account_type, int.Parse(DataSource.LookupDAL.LookupCurrencyISONumericCode(customerDetails.CurrencyId.Value)), customerDetails.CustomerIDNumber, dob,
            //       address, postCode, comment, auditUserId, auditWorkStation, out responseMessage))


            //            _cmsLog.Debug("PAN =" + PAN + "for customer=" + customerDetails.CardReference);
            //        if (bankWorldService.CreateCard(customerDetails, card.card_expiry_date, cardType, cashLimit, address, serviceCode, languageId, out responseMessage))

            //            return LinkResponse.SUCCESS;
            //        else
            //            return LinkResponse.ERROR;
            //    }



            //    catch (System.ServiceModel.EndpointNotFoundException endpointException)
            //    {
            //        _cmsLog.Error(endpointException);
            //        responseMessage = "Unable to connect to FIMI, please try again or contact support.";
            //        return LinkResponse.RETRY;
            //    }
            //    catch (Exception ex)
            //    {
            //        _cmsLog.Error(ex);
            //        responseMessage = ex.Message;
            //    }

            //    return LinkResponse.ERROR;
            //}

            ///---------------------------------------------------ABOVE FOR MIGRATION----------------------------------------------------

            try
            {
                responseMessage = String.Empty;

                //ParametersDAL pDal = new ParametersDAL(this.SQLConnectionString);
                //LookupDAL lDal = new LookupDAL(this.SQLConnectionString);

                //First get parameter for CMS web service call
                //var cmsParms = pDal.GetParameter(customerDetails.IssuerId, 1, 1, null, auditUserId, auditWorkStation);

                //if (cmsParms == null)
                //    throw new ArgumentNullException("Cannot find parameters for Card Management System.");

                if (!(config is WebServiceConfig))
                {
                    _cmsLog.Error("CMS config parameters must be for Webservice.");
                    responseMessage = "CMS config parameters must be for Webservice.";

                    return LinkResponse.RETRY;
                }

                BankWorldWebService bankWorldService = new BankWorldWebService((WebServiceConfig)config, DataSource);

                if (!bankWorldService.UpdateCard(customerDetails, languageId, out responseMessage))
                    return LinkResponse.ERROR;

                //WriteToLog("UpdatePVV: PAN=" + customerDetails.CardNumber + ", PVVOffset=" + customerDetails.PinOffset);

                //if (!bankWorldService.UpdatePVV(customerDetails.CardNumber, customerDetails.PinOffset, languageId, out responseMessage))
                //    return LinkResponse.ERROR;

                if (!bankWorldService.ActivateCard(customerDetails.CardNumber, "NORMAL", languageId, out responseMessage))
                    return LinkResponse.ERROR;

                responseMessage = "Success";
                return LinkResponse.SUCCESS;
            }
            catch (System.ServiceModel.EndpointNotFoundException endpointException)
            {
                _cmsLog.Error(endpointException);
                responseMessage = "Unable to connect to BankWorld, please try again or contact support.";
                return LinkResponse.RETRY;
            }
            catch (Exception ex)
            {
                _cmsLog.Error(ex);
                responseMessage = ex.Message;
            }

            return LinkResponse.ERROR;
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
            /////-----------------------MIGRATION BELOW-------------------------------------------
            //_cmsLog.Debug("caaling upload method in Fidelity CMS");
            //EMPCMS empCMS = new EMPCMS(this.DataSource);
            //responseMessage = String.Empty;
            //_cmsLog.Debug("caaling upload method in Fidelity CMS step 2");
            //empCMS.UploadGeneratedCards(cardObjects, externalFields, config, languageId, auditUserId, auditWorkStation, out string rspMessages);
            //_cmsLog.Debug("caaling upload method in Fidelity CMS step 3");
            //responseMessage = rspMessages;
            //return true;
            ///-------------------------------ABOVE MIGRATION-----------------------------

            //disabled for the NI migration
            //ParametersDAL pDal = new ParametersDAL(this.SQLConnectionString);
            //ConfigDAL configDal = new ConfigDAL(this.SQLConnectionString);
            CardWorld.CardWorld cardWorld = new CardWorld.CardWorld(this.DataSource);
            FilePath filePath = new FilePath();

            DateTime runDateTime = DateTime.Now;
            responseMessage = String.Empty;
            bool succ = true;

            _cmsLog.Debug("calling GenerateCardNumbers");
            GenerateCardNumbers(ref cardObjects, languageId, auditUserId, auditWorkStation, out responseMessage);
            _cmsLog.Debug("step2");
            foreach (var productId in cardObjects.Select(s => s.ProductId).Distinct().ToList())
            {
                //config = configDal.GetProductInterfaceConfig(productId, 1, 0, null, -2, "SYSTEM");

                FileSystemConfig fileConfig;

                if (config is Config.FileSystemConfig)
                    fileConfig = (FileSystemConfig)config;
                else
                    throw new ArgumentException("Must be file system config.", "config");

                //Parameters param = new Parameters("", "", 0, null, null, "", "", null);
                //var param = pDal.GetParameter(issuerId, 1, 0, null, auditUserId, auditWorkStation);

                //If multiple paths found, use first for first file, second for second etc
                //var paths = param.Path.Split(';');
                //var fileNames = param.FileName.Split(';');
                var paths = fileConfig.Path.Split(';');
                var filenames = fileConfig.Filename.Split(';');
                _cmsLog.Debug("step3");
                int cardissuemethodId = 1;
                if (cardObjects.Count > 0)
                {
                    cardissuemethodId = cardObjects[0].CardIssueMethodId;
                }
                _cmsLog.Debug("cardissuemethodId" + cardissuemethodId);
                try
                {
                    //Write out first file
                    var xmlFileDirs = filePath.CreateDirectory(paths[0], cardObjects.Where(w => w.ProductId == productId).ToList(), runDateTime);

                    foreach (var directory in xmlFileDirs)
                    {
                        _cmsLog.Debug("directory" + directory);
                        var filename = filePath.Filename(filenames[0], directory.Value[0], runDateTime);
                        cardWorld.GenerateFiles(directory.Value, directory.Key, filename, externalFields);
                        _cmsLog.Debug("completed generated files");
                    }


                    if (cardissuemethodId == 0)// if it is centeralised issuing we are generating branchfile
                    {
                        var textFileDirs_1 = filePath.CreateDirectory(paths[0], cardObjects.Where(w => w.ProductId == productId).ToList(), runDateTime);
                        _cmsLog.Debug("textFileDirs_1" + textFileDirs_1);
                        foreach (var directory in textFileDirs_1)
                        {
                            _cmsLog.Debug("directory centralised" + directory);
                            var filename = filePath.Filename(filenames.Length > 1 ? filenames[1] : filenames[0], directory.Value[0], runDateTime);

                            GenerateBranchFile(directory.Value, directory.Key, filename);
                        }
                    }
                    else // for instant card we are generating the card refernce file.
                    {
                        //Write out second file
                        _cmsLog.Debug("calling Write out second file");
                        var textFileDirs = filePath.CreateDirectory(paths[0], cardObjects.Where(w => w.ProductId == productId).ToList(), runDateTime);

                        foreach (var directory in textFileDirs)
                        {
                            _cmsLog.Debug("directory instant" + directory);
                            var filename = filePath.Filename(filenames.Length > 1 ? filenames[1] : filenames[0], directory.Value[0], runDateTime);
                            _cmsLog.Debug("directory instant" + filename);
                            //string path = Path.Combine(directory.Key, filename);
                            _cmsLog.Debug("calling generatecardtextfile");
                            cardWorld.GenerateCardTextFile(directory.Value, directory.Key, filename);
                        }
                    }


                }
                catch (IOException ioe)
                {
                    responseMessage += ioe.Message + "<br />";
                    succ = false;
                }
            }

            return succ;
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

            //SequenceNumberDAL sDAL = new SequenceNumberDAL(this.SQLConnectionString);

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
            //this.WriteToLog("Branch File finished");
        }
        public bool ValidateCustomerDetails(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            responseMessage = String.Empty;
            return true;
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

        #region Private Card Number Generator
        private bool GenerateCardNumbers(ref List<CardObject> cardObjects, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            //if (String.IsNullOrWhiteSpace(SQLConnectionString))
            //    throw new ArgumentNullException("SQL Connection string property not set.");


            //cardGeneratorDAL = new CardGeneratorDAL(SQLConnectionString);
            _cmsLog.Debug("calling productSequence");
            var productSequence = GetProductSequenceDictionary(cardObjects.Select(s => s.ProductId).Distinct().ToList(), auditUserId, auditWorkStation);
            _cmsLog.Debug("productSequence " + productSequence);

            //Loop through all the cards and generate a unique card number
            foreach (var card in cardObjects)
            {
                if (!card.CardNumber.StartsWith(card.BIN))
                {
                    var seq = productSequence[card.ProductId];

                    card.CardNumber = CardNumberGenerator.GenerateCardNumber(card, seq.CurrentSequence, 19);
                    seq.Increment();
                    productSequence[card.ProductId] = seq;
                }
                _cmsLog.Debug("card.CardNumber" + card.CardNumber);
            }

            UpdateCards(cardObjects, productSequence.Values.ToList(), auditUserId, auditWorkStation);

            responseMessage = "";
            return true;
        }

        /// <summary>
        /// Fetch All the Products and the last sequence they were assigned. The sequence number returned will be used for the new card number
        /// </summary>
        /// <param name="productIdList"></param>
        /// <param name="SQLConnectionString"></param>
        /// <param name="auditUserId"></param>
        /// <param name="auditWorkStation"></param>
        /// <returns></returns>
        private Dictionary<int, ProductSequenceCount> GetProductSequenceDictionary(List<int> productList, long auditUserId, string auditWorkStation)
        {
            Dictionary<int, ProductSequenceCount> productSequence = new Dictionary<int, ProductSequenceCount>();
            _cmsLog.Debug("calling productSequence Dictionary");
            //Find all the distinct products that might have been passed through from the card list, 
            //each product has it's own sequence
            foreach (var product in productList)
            {
                _cmsLog.Debug("calling productSequence Dictionary " + product);
                if (!productSequence.ContainsKey(product))
                {
                    var lastSeq = DataSource.CardGeneratorDAL.GetLatestSequenceNumber(product, auditUserId, auditWorkStation);
                    _cmsLog.Debug("lastSeq " + lastSeq);
                    if (lastSeq >= 0)
                        productSequence.Add(product, new ProductSequenceCount(product, lastSeq));
                    _cmsLog.Debug("lastSeq " + lastSeq + "added to product sequence");
                }
            }

            return productSequence;
        }

        private void UpdateCards(List<CardObject> cardObjects, List<ProductSequenceCount> productSequences, long auditUserId, string auditWorkStation)
        {
            _cmsLog.Debug("calling update card");
            Dictionary<long, string> cards = new Dictionary<long, string>();
            foreach (var cardObject in cardObjects)
            {
                cards.Add(cardObject.CardId, cardObject.CardNumber);
            }

            Dictionary<int, int> sequences = new Dictionary<int, int>();
            foreach (var seq in productSequences)
            {
                sequences.Add(seq.ProductId, seq.CurrentSequence);
            }
            _cmsLog.Debug("calling updatecardandSequencenumber");
            DataSource.CardGeneratorDAL.UpdateCardsAndSequenceNumber(cards, sequences, auditUserId, auditWorkStation);
            _cmsLog.Debug("done calling updatecardandSequencenumber");
        }
        #endregion        

        [Conditional("DEBUG")]
        private void WriteToLog(string message)
        {
            if (_cmsLog.IsDebugEnabled)
                _cmsLog.Debug(message);
        }

        public bool RemoteFetchDetails(List<CardDetail> cardDetails, ExternalSystemFields externalFields, IConfig config, out List<CardDetailResponse> failedCards, out string responseMessage)
        {
            throw new NotImplementedException();
        }

        public LinkResponse LinkCardsToAccount(List<CustomerDetails> customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out Dictionary<long, LinkResponse> response, out string responseMessage)
        {
            throw new NotImplementedException();
        }


        public LinkResponse ActiveCard(CustomerDetails customerDetails, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            //        ///-------------------------------FOR MIGRATION-----------------------------------------------
            //        responseMessage = String.Empty;

            //        try
            //        {
            //            WebServiceConfig webConfig = null;
            //            if (config is WebServiceConfig)
            //                webConfig = (WebServiceConfig)config;
            //            else
            //                throw new ArgumentException("Config parameters must be for Webservice.");

            //            //split address field for bankworld and NI cms ;
            //            string fullAddress = webConfig.Address;
            //            _cmsLog.Debug("fullAddress =" + fullAddress);
            //            string[] details = fullAddress.Split(';');
            //            _cmsLog.Debug("details =" + details);
            //            string bankworldAddress = string.Empty;
            //            string niAddress = string.Empty;
            //            //string niPort = string.Empty;
            //            if (details == null)
            //            {
            //                responseMessage = "Address field is not conigured for Bankworld or NI";
            //            }
            //            else
            //            {
            //                if (details.Length == 2)
            //                {
            //                    bankworldAddress = details[0];
            //                    niAddress = details[1];
            //                    // niPort = details[2];
            //                    _cmsLog.Debug("bankworldAddress =" + bankworldAddress);
            //                    _cmsLog.Debug("niAddress =" + niAddress);
            //                    //_cmsLog.Debug("restPath =" + niPort);
            //                }
            //                else
            //                {
            //                    responseMessage = "bankworld Address;NIAddress are not configured correctly for CMS";
            //                    throw new ArgumentException("bankworld Address;NIAddress are not configured correctly for CMS");
            //                }
            //            }
            //            string niUsername = webConfig.RemoteUsername;
            //            string niPassword = webConfig.RemotePassword;
            //            int niPort = Convert.ToInt16(webConfig.RemotePort);


            //            FIMIWebService fimiService = new FIMIWebService(niAddress, "", niPort, niUsername, niPassword, new DefaultDataDAL(this.DataSource));
            //            BankWorldWebService bankWorldService = new BankWorldWebService(bankworldAddress, webConfig.Path, webConfig.Port, webConfig.Username, webConfig.Password, DataSource);

            //            string fullname = customerDetails.FirstName;

            //            if (!String.IsNullOrWhiteSpace(customerDetails.MiddleName))
            //                fullname += " " + customerDetails.MiddleName;

            //            if (!String.IsNullOrWhiteSpace(customerDetails.LastName))
            //                fullname += " " + customerDetails.LastName;

            //            string branchCode = DataSource.LookupDAL.LookupEmpBranchCode(customerDetails.BranchId);

            //            //string branchCode = branchCodes[customerDetails.BranchId];

            //            if (String.IsNullOrWhiteSpace(branchCode))
            //                throw new ArgumentNullException("EMPBranchCode", "Branch does not have an EMP branch code specified.");


            //            DateTime dob = DateTime.Now;
            //            string address = String.Empty;
            //            string postCode = String.Empty;

            //            if (customerDetails.ProductFields != null && customerDetails.ProductFields.Count > 0)
            //            {
            //                foreach (var field in customerDetails.ProductFields)
            //                {
            //                    if (field.MappedName.Equals("ind_sys_dob", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        string dateofBirth = Encoding.ASCII.GetString(field.Value);
            //                        dob = DateTime.ParseExact(dateofBirth, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            //                        // DateTime.Parse(Encoding.UTF8.GetString(field.Value),CultureInfo.InvariantCulture);
            //                        _cmsLog.Debug("EMPDOB field" + dob);
            //                    }

            //                    if (field.MappedName.Equals("ind_sys_address", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        address = Encoding.UTF8.GetString(field.Value);
            //                        _cmsLog.Debug("EMP ADDRESS " + address);
            //                    }

            //                    if (field.MappedName.Equals("ind_sys_postcode", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        postCode = Encoding.UTF8.GetString(field.Value);
            //                        _cmsLog.Debug("EMP Postal Code" + postCode);
            //                    }
            //                }
            //            }

            //            // Comment field is built up by emp product account type and currency, and branch code
            //            // Each product's currency must have the emp_account_type variable set.

            //            string empAccountType = String.Empty;
            //            if (!customerDetails.CurrencyFields.TryGetValue("emp_account_type", out empAccountType))
            //            {
            //                responseMessage = String.Format("emp_account_type not set for product currency {0}. Please check product currency configuration.", customerDetails.CurrencyCode);
            //                return LinkResponse.RETRY;
            //            }

            //            string comment = String.Format("{0}~{1}", empAccountType, branchCode);

            //            //string comment = "~";
            //            //if (externalFields.Field != null)
            //            //    externalFields.Field.TryGetValue("comment", out comment);
            //            int cms_account_type;
            //            if (!string.IsNullOrEmpty(customerDetails.CMSAccountType))
            //            {
            //                int.TryParse(customerDetails.CMSAccountType, out cms_account_type);
            //            }
            //            else
            //            {
            //                throw new Exception("CMSAccountType is null");
            //            }
            //            var item = externalFields.Field.FirstOrDefault(i => i.Key == "cardType");
            //            if (item.Key == null)
            //            {
            //                throw new Exception("cardType external field not found.");
            //            }
            //            item = externalFields.Field.FirstOrDefault(i => i.Key == "cashLimit");
            //            if (item.Key == null)
            //            {
            //                throw new Exception("cashLimit external field not found.");
            //            }
            //            item = externalFields.Field.FirstOrDefault(i => i.Key == "serviceCode");
            //            if (item.Key == null)
            //            {
            //                throw new Exception("serviceCode external field not found.");
            //            }
            //            string cardType = externalFields.Field["cardType"].ToString();
            //            string cashLimit = externalFields.Field["cashLimit"].ToString();
            //            string serviceCode = externalFields.Field["serviceCode"].ToString();
            //            CardDetail card = new CardDetail();
            //            ////added below to get the pan to update bankworld for Fidelity
            //            if (fimiService.FetchPan(int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, customerDetails.CustomerIDNumber, out string PAN, out DateTime? expiryDate, out responseMessage))
            //                customerDetails.CardNumber = PAN;
            //            card.card_expiry_date = expiryDate;
            //            _cmsLog.Debug("PAN==" + customerDetails.CardNumber);
            //            _cmsLog.Debug("expiryDat==e" + expiryDate);
            //            //// Added till here for Fidelity to update bankworld

            //            if (fimiService.LinkCardToAccountAndActive(customerDetails.CardId, int.Parse(customerDetails.CardReference), customerDetails.IssuerCode, fullname, customerDetails.AccountNumber,
            //           cms_account_type, int.Parse(DataSource.LookupDAL.LookupCurrencyISONumericCode(customerDetails.CurrencyId.Value)), customerDetails.CustomerIDNumber, dob,
            //           address, postCode, comment, auditUserId, auditWorkStation, out responseMessage))

            //                if (bankWorldService.CreateCard(customerDetails, card.card_expiry_date, cardType, cashLimit, address, serviceCode, languageId, out responseMessage))

            //                    return LinkResponse.SUCCESS;
            //                else
            //                    return LinkResponse.ERROR;
            //        }



            //        catch (System.ServiceModel.EndpointNotFoundException endpointException)
            //        {
            //            _cmsLog.Error(endpointException);
            //            responseMessage = "Unable to connect to FIMI, please try again or contact support.";
            //            return LinkResponse.RETRY;
            //        }
            //        catch (Exception ex)
            //        {
            //            _cmsLog.Error(ex);
            //            responseMessage = ex.Message;
            //        }

            //        return LinkResponse.ERROR;
            //    }

            //    ///---------------------------------------------------ABOVE FOR MIGRATION----------------------------------------------------

            BankWorldWebService bankWorldService = new BankWorldWebService((WebServiceConfig)config, DataSource);
            if (!bankWorldService.ActivateCard(customerDetails.CardNumber, "NORMAL", languageId, out responseMessage))
                return LinkResponse.ERROR;

            responseMessage = "Success";
            return LinkResponse.SUCCESS;
        }
    }
}

