using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Veneka.Module.CR2CardWorld;
using Veneka.Module.CR2CardWorld.Objects;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Objects;
using System.IO;
using Veneka.Indigo.Integration.Fidelity.BranchFile.Objects;
using Veneka.Indigo.Integration.Fidelity.Util;
using Veneka.Indigo.Integration.ProductPrinting;
using Veneka.Indigo.Integration.External;
using Veneka.Indigo.Integration.Fidelity.BranchFile;
using Veneka.Indigo.Integration.Fidelity.DAL;

namespace Veneka.Indigo.Integration.Fidelity.CardWorld
{
    public class CardWorld
    {
        private readonly CardWorldService _cardWorldService;
        private static readonly ILog _cmsLog = LogManager.GetLogger(General.CMS_LOGGER);
        //private string _connentionstring;
        public IDataSource DataSource { get; set; }
        public CardWorld(IDataSource _datasource)
        {
            DataSource = _datasource;
            _cardWorldService = new CardWorldService(new DefaultDataDAL(this.DataSource), new ValidationDAL(this.DataSource), General.CMS_LOGGER);
           
        }

        //Generate 3rd party system files.
        public void GenerateFiles(List<CardObject> cardObjects, string outputDirectory, string fileName, ExternalSystemFields externalfields)
        {
            _cmsLog.Debug("GenerateFiles");
            if (!outputDirectory.EndsWith("\\"))
                outputDirectory += "\\";

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = "xml_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".xml";
                _cmsLog.Debug("fileName" + fileName);
            }

            if (!fileName.EndsWith(".xml"))
                fileName += ".xml";

            FileInfo fileInfo = new FileInfo(Path.Combine(outputDirectory, fileName));

            _cmsLog.Debug("SerializeCardBatch");
            _cmsLog.Debug("fileInfo.DirectoryName" + fileInfo.DirectoryName);
            _cardWorldService.SerializeCardBatch(BuildCardBatchObject(cardObjects, externalfields), fileInfo.DirectoryName, fileInfo.Name, true);

            //GenerateCardTextFile(cardObjects, fileInfo.DirectoryName);
        }

        /// <summary>
        /// Converts from what Indigo understand to what the module understands.
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private CardBatch BuildCardBatchObject(List<CardObject> cards, ExternalSystemFields externalfields)
        {
            _cmsLog.Debug("calling build card batch method");
            CardBatch cardBatch = new CardBatch();
            List<CardRecord> cardRecords = new List<CardRecord>();

            Account account = null; ;

            foreach (var card in cards)
            {
                account = new Account();
                if (String.IsNullOrWhiteSpace(card.CardNumber))
                    throw new ArgumentNullException("Card number cannot be null or empty");

                if (String.IsNullOrWhiteSpace(card.BranchCode))
                    throw new ArgumentNullException("Branch code cannot be null or empty");

                CardRecord cardRecord = new CardRecord();
                _cmsLog.Debug("calling build card batch method step1");

                if (card.CardIssueMethodId == 0 && card.CustomerAccount != null)
                {
                    account.AccountId = card.CustomerAccount.AccountNumber;
                    account.AccountDesc = EncodeAccountType(card.CustomerAccount.AccountTypeId);
                    cardRecord.Branch = card.DeliveryBranchCode;
                    cardRecord.EncodedName = BuildEncodedName(card.CustomerAccount.NameOnCard);
                    cardRecord.CardExpiry = DateTime.Now.AddMonths(card.ExpiryMonths).ToString("yyMM");
                    foreach (var printField in card.PrintFields)
                    {
                        if (printField is ProductField)
                        {

                            switch (printField.MappedName.ToUpper().Trim())
                            {
                                case "ADDR1":
                                    cardRecord.Addr1 = System.Text.Encoding.UTF8.GetString(((ProductField)printField).Value);
                                    break;
                                case "ADDR2":
                                    cardRecord.Addr2 = System.Text.Encoding.UTF8.GetString(((ProductField)printField).Value);
                                    break;
                                case "ADDR3":
                                    cardRecord.Addr3 = System.Text.Encoding.UTF8.GetString(((ProductField)printField).Value);
                                    break;
                            }

                        }
                    }

                }
                else
                {
                    account.AccountId = externalfields.Field["AccountId"].ToString();
                    cardRecord.EncodedName = externalfields.Field["EncodedName"].ToString();
                    cardRecord.Branch = externalfields.Field["Branch"].ToString();
                    account.AccountDesc = externalfields.Field["AccountDesc"].ToString();



                }
                cardRecord.action = externalfields.Field["action"].ToString();
                account.AccountSystem = externalfields.Field["AccountSystem"].ToString();

                cardRecord.Accounts = new Account[] { account };

                //Set Other data
                cardRecord.CardNumber = card.CardNumber;
                cardRecord.CardSeqNumber = 0;




                cardRecords.Add(cardRecord);
            }
            _cmsLog.Debug("calling build card batch method step2");
            cardBatch.CardRecords = cardRecords.ToArray();

            return cardBatch;
        }



        private string BuildEncodedName(string custname)
        {
            string[] splitName = custname.Trim().Split();
            string lastName = string.Empty;
            string firstname = string.Empty;
            string middlename = string.Empty;

            lastName = splitName[splitName.Length - 1].Trim();

            for (int i = 0; i < splitName.Length - 1; i++)
            {
                if (!String.IsNullOrWhiteSpace(splitName[i]))
                {
                    if (String.IsNullOrWhiteSpace(firstname) && i == 0)
                        firstname = splitName[i];
                    else
                        middlename += splitName[i] + " ";
                }

            }

            return lastName.Trim() + "/" + firstname.Trim() + " " + middlename.Trim();

            //if (splitName.Length > 2)
            //{
            //    middlename = splitName[1].Trim();
            //    lastName = splitName[2].Trim();
            //    firstname = splitName[0].Trim();
            //}
            //else if (splitName.Length > 1)
            //{
            //    lastName = splitName[splitName.Length - 1].Trim();
            //    firstname = splitName[0].Trim();
            //}
            //else if (splitName.Length > 0)
            //{
            //    firstname = splitName[0].Trim();
            //}

            //if (!string.IsNullOrEmpty(lastName))
            //{
            //    return lastName + "/" + firstname+" "+middlename;
            //}
            //return firstname;

        }
        /// <summary>
        /// Generates Card Textfile. This is used at Fidelity for printing on the card stock envelops
        /// </summary>
        /// <param name="cardObjects"></param>
        /// <param name="outputDirectory"></param>
        public void GenerateCardTextFile(List<CardObject> cardObjects, string outputDirectory, string fileName)
        {
            _cmsLog.Debug("GenerateCardTextFile");
            if (!outputDirectory.EndsWith("\\"))
                outputDirectory += "\\";

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = "text_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
            }

            if (!fileName.EndsWith(".txt"))
                fileName += ".txt";

            string path = Path.Combine(outputDirectory, fileName);
            FileInfo fileInfo = new FileInfo(path);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            //if (fileInfo.Exists)
            if (File.Exists(path))
                throw new IOException(fileInfo + " Already Exists!");

            using (StreamWriter file = new StreamWriter(fileInfo.OpenWrite()))
            {
                foreach (var card in cardObjects)
                {
                    file.WriteLine(card.CardNumber + "," + card.CardReferenceNumber);
                    _cmsLog.Debug("card.CardNumber" + card.CardNumber + "," + card.CardReferenceNumber);
                }
            }
        }

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
