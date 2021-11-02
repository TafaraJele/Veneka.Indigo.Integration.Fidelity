using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Module.IntegrationDataControl;
using Veneka.Module.IntegrationDataControl.DAL;

namespace Veneka.Indigo.Integration.Fidelity.DAL
{
    public class DefaultDataDAL: IDefaultDataDAL
    {
        
        public IDataSource _defaultdatasource;
        public DefaultDataDAL(IDataSource dataSource)
        {
            _defaultdatasource = dataSource;
        }
        public List<IntegrationField> FetchDefaultValues(string integrationName, string objectName)
        {
            Dictionary<string, string> inputparameters = new Dictionary<string, string>();
            Dictionary<string, string> outputparameters = new Dictionary<string, string>();

            List<IntegrationField> resp = new List<IntegrationField>();

            inputparameters.Add("integration_name", integrationName);
            inputparameters.Add("integration_object_name", objectName);

            outputparameters.Add("integration_id", string.Empty);
            outputparameters.Add("integration_object_id", string.Empty);
            outputparameters.Add("integration_field_id", string.Empty);
            outputparameters.Add("integration_field_name", string.Empty);
            outputparameters.Add("integration_field_default_value", string.Empty);

            List<Dictionary<string, string>> response = _defaultdatasource.CustomDataDAL.DataCall("usp_integration_get_default_values", inputparameters, outputparameters);

            foreach (Dictionary<string, string> item in response)
            {
                resp.Add(new IntegrationField(int.Parse(item["integration_id"].ToString()),
                                                int.Parse(item["integration_object_id"].ToString()),
                                                int.Parse(item["integration_field_id"].ToString()),
                                                item["integration_field_name"].ToString(),
                                                item["integration_field_default_value"].ToString()));

            }
            return resp;
        }
    }
}

