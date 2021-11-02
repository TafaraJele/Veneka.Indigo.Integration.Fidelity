using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veneka.Indigo.Integration;
using System.ComponentModel.Composition;
using Common.Logging;
using Veneka.Indigo.Integration.Config;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.External;
using System.IO;
using Veneka.Indigo.Integration.Common;

namespace Veneka.Indigo.Integration.Fidelity
{
    [IntegrationExport("FidelityCPS", "01D78A39-465A-41FD-B27D-907A57B5AA2C", typeof(ICardProductionSystem))]
    class FidelityCPS : ICardProductionSystem
    {
        private static readonly ILog _cpsLog = LogManager.GetLogger(General.CPS_LOGGER);



        public string SQLConnectionString { get; set; }
        public System.IO.DirectoryInfo IntegrationFolder { get; set; }
        public IDataSource DataSource { get; set; }

        public bool UploadToCardProduction(ref List<CardObject> cardObjects, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            throw new NotImplementedException();
        }

        public bool UploadToCardProduction(ref List<CardObject> cardObjects, ExternalSystemFields externalFields, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
        {
            throw new NotImplementedException();
        }
    }
}
