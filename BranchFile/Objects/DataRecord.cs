using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Indigo.Integration.Fidelity.Util;

namespace Veneka.Indigo.Integration.Fidelity.BranchFile.Objects
{
    public class DataRecord : FileRecord
    {
        private string _bin = string.Empty;
        private string _card_number = string.Empty;
        private string _card_refrence_number = string.Empty;
        private string _branch_batch_referencenumber = string.Empty;
        //private string _batchId ;
        private string _branch_name = string.Empty;
        private string _branch_code = string.Empty;
        private string delimiter = ",";
        public DataRecord(string bin, string card_number, string card_refrencenumber, string branch_batch_referencenumber, string branch_name, string branch_code)
        {

            this._bin = bin;
            this._card_number = card_number;
            this._card_refrence_number = card_refrencenumber;
            this._branch_batch_referencenumber = branch_batch_referencenumber;
            this._branch_name = branch_name;
            this._branch_code = branch_code;

        }

        public override string OutputLine()
        {

            return Bin + delimiter +
                Card_number + delimiter +
                Card_refrence_number + delimiter +
                Branch_batch_reference + delimiter +
                Branch_code + delimiter +
                Branch_name;
        }

        public override string CheckForNull(string field)
        {
            throw new NotImplementedException();
        }

        private string FormatField(string value, int expectedlength)
        {
            int actuallength = value.Count();
            if (actuallength != expectedlength)
            {

                if (actuallength < expectedlength)
                {
                    value = value.PadRight(expectedlength);
                }

            }
            return value;

        }


        public string Bin
        {
            get
            {
                return _bin;
            }
        }
        public string Card_number
        {

            get
            {

                return _card_number;

            }
        }
        public string Card_refrence_number
        {

            get
            {

                return _card_refrence_number;

            }
        }
        public string Branch_batch_reference
        {

            get
            {

                return _branch_batch_referencenumber;

            }
        }
        public string Branch_name
        {

            get
            {

                return _branch_name;

            }
        }


        public string Branch_code
        {

            get
            {

                return _branch_code;

            }
        }
    }
}
