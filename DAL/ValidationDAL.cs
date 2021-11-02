using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Module.IntegrationDataControl.DAL;
namespace Veneka.Indigo.Integration.Fidelity.DAL
{
    public class ValidationDAL : IValidationDAL
    {
        public IDataSource _defaultdatasource;
        public ValidationDAL(IDataSource dataSource)
        {
            _defaultdatasource = dataSource;
        }
        public void FetchAccountValidations(List<ValidationField> validationFields, int languageId)
        {
            Dictionary<Tuple<int, int, int>, string> fieldsDict = new Dictionary<Tuple<int, int, int>, string>();

            foreach (var field in validationFields)
            {
                var key = Tuple.Create<int, int, int>(field.Integrationid, field.ObjectId, field.FieldId);

                if (!fieldsDict.ContainsKey(key))
                    fieldsDict.Add(key, field.Value);
            }

            Dictionary<string, string> inputparameters = new Dictionary<string, string>();
            Dictionary<string, string> outputparameters = new Dictionary<string, string>();

            List<ValidationField> resp = new List<ValidationField>();
            Dictionary<string, DataTable> inputkeytablevalue = new Dictionary<string, DataTable>();

            inputkeytablevalue.Add("field_list", CreateKeyValueTable(fieldsDict));
            inputparameters.Add("language_id", languageId.ToString());

            outputparameters.Add("integration_id", string.Empty);
            outputparameters.Add("integration_object_id", string.Empty);
            outputparameters.Add("integration_field_id", string.Empty);
            outputparameters.Add("response_text", string.Empty);
            outputparameters.Add("integration_response_valid_response", string.Empty);

            //List<Dictionary<string, string>> response = _defaultdatasource.CustomDataDAL.DataCall("usp_integration_get_response_fields", inputparameters, inputkeytablevalue, outputparameters);
            List<Dictionary<string, string>> response = _defaultdatasource.CustomDataDAL.DataCall("usp_integration_get_response_fields", inputparameters,outputparameters);

            foreach (Dictionary<string, string> reader in response)
            {
                int integrationId = int.Parse(reader["integration_id"].ToString());
                int objectId = int.Parse(reader["integration_object_id"].ToString());
                int fieldId = int.Parse(reader["integration_field_id"].ToString());
                var field = validationFields.Where(w => w.Integrationid == integrationId &&
                                                               w.ObjectId == objectId &&
                                                               w.FieldId == fieldId)
                                                   .First();

                if (field != null)
                {
                    field.isValid = bool.Parse(reader["integration_response_valid_response"].ToString());
                    field.ValidMessage = reader["response_text"].ToString();
                }
            }


        }
    

        public List<ValidationField> FetchFieldsToValidate(string integrationName, string objectName)
        {
            Dictionary<string, string> inputparameters = new Dictionary<string, string>();

            Dictionary<string, string> outputparameters = new Dictionary<string, string>();

            List<ValidationField> resp = new List<ValidationField>();

            inputparameters.Add("integration_name", integrationName);
            inputparameters.Add("integration_object_name", objectName);

            outputparameters.Add("integration_id", string.Empty);
            outputparameters.Add("integration_object_id", string.Empty);
            outputparameters.Add("integration_field_id", string.Empty);
            outputparameters.Add("integration_field_name", string.Empty);
            outputparameters.Add("accept_all_responses", string.Empty);

            List<Dictionary<string, string>> response = _defaultdatasource.CustomDataDAL.DataCall("usp_integration_get_response_fields", inputparameters, outputparameters);

            foreach (Dictionary<string, string> item in response)
            {
                resp.Add(new ValidationField(int.Parse(item["integration_id"].ToString()),
                                                        int.Parse(item["integration_object_id"].ToString()),
                                                        int.Parse(item["integration_field_id"].ToString()),
                                                        item["integration_field_name"].ToString(),
                                                        bool.Parse(item["accept_all_responses"].ToString())));

            }
            return resp;
        }

        #region Private Methods
        private DataTable CreateKeyValueTable(Dictionary<Tuple<int, int, int>, string> dictionary)
        {
            string key1 = "key1";
            string key2 = "key2";
            string key3 = "key3";
            string value = "value";

            DataTable dt = new DataTable();
            dt.Columns.Add(key1, typeof(long));
            dt.Columns.Add(key2, typeof(long));
            dt.Columns.Add(key3, typeof(long));
            dt.Columns.Add(value, typeof(string));

            foreach (var item in dictionary)
            {
                dt.Rows.Add(CreateTriKeyRow(item.Key, item.Value, dt.NewRow()));
            }

            return dt;
        }

        private DataRow CreateTriKeyRow(Tuple<int, int, int> key, string value, DataRow workRow)
        {
            workRow["key1"] = key.Item1;
            workRow["key2"] = key.Item2;
            workRow["key3"] = key.Item3;

            workRow["value"] = value;

            return workRow;
        }
        #endregion
    }
}
