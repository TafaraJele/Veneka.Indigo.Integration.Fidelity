using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Veneka.Indigo.Integration.Common;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Objects;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FIDELITY3DSECURE", "C4EB1F61-8C09-449D-A44D-28C9E15AC868", typeof(I3DSecureRegistration))]
    public class FidelityThreedSecure : I3DSecureRegistration
    {

        private readonly ILog _threedfileLoaderLog = LogManager.GetLogger(General.FILE_LOADER_LOGGER);
        public string BaseFileDir { get; set; }
        public string SQLConnectionString { get; set; }
        public IDataSource DataSource { get; set; }
        public DirectoryInfo IntegrationFolder { get; set; }

        public bool Generate3DSecureFiles(List<ThreeDSecureCardDetails> threeDSecureDetails, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {

            if (threeDSecureDetails != null)
            {

                XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Root", from item in threeDSecureDetails
                                         select

                  new XElement("Record",
                //new XElement("PAN", item.CardNumber),
                //new XElement("CANCELDATE", item.CardExpiryDate.Value.ToString("dd/MM/yyyy")),
                //new XElement("MBR", "0"),
                new XElement("ACCOUNT", item.CustomerAccountNumber),
                new XElement("CELLPHONE", item.ContactNumber.StartsWith("+") ? item.ContactNumber.Substring(1) : item.ContactNumber),
                new XElement("EMAIL", item.ContactEmail))));


                string prefix_bin = threeDSecureDetails[0].CardNumber.Substring(0, 6);
                string prefix_bank_id = "FBPG";
                string partFileName = "Update Mobile & Email By AccountNo";

                if (config is FileSystemConfig)
                {
                    var parms = (FileSystemConfig)config;

                    //string timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                    //string filename =Path.Combine(parms.Path + "VISA CLASSIC"+ timeStamp + ".xml");
                    // string filename = Path.Combine(parms.Path + @"\CustomerData" + timeStamp + ".xml");

                    string filename = String.Format("{0}{1}_{2}_{3}.xml", parms.Path + @"\", prefix_bank_id, prefix_bin, partFileName);
                    //string filename = Path.Combine(parms.Path + @"\" + prefix_bank_id + prefix_bank_id + + ".xml");
                    _threedfileLoaderLog.Debug("Filepath =" + filename);
                    xdoc.Save(filename);
                }
                responseMessage = ("File created Successfully");
                return true;

            }
            else
            {
                responseMessage = ("Failed to create file");
                return false;

            }
        }
    }
}
