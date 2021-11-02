using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using System.Globalization;
using Veneka.Indigo.Integration.Fidelity.Util;

namespace Veneka.Indigo.Integration.Fidelity.BranchFile.Objects
{

    public class HeaderRecord : FileRecord
    {
        private string _issuer = string.Empty;
        private string _card_program = string.Empty;
        private string _sequence_number = string.Empty;
        private DateTime _datetime;
        //private string _batchId ;
        private string _production_batch_reference = string.Empty;
        private string delimiter = ",";


        public HeaderRecord(string issuer, string card_program, DateTime datetime, string sequence_number, string production_batch_reference)
        {

            this._issuer = issuer;
            this._card_program = card_program;
            this._sequence_number = sequence_number;
            this._production_batch_reference = production_batch_reference;
            this._datetime = datetime;

        }

        public override string OutputLine()
        {
            string _header_datetime;
            CultureInfo InvC = CultureInfo.InvariantCulture;
            _header_datetime = CreatedDataTime.ToString("YYYYMMDD", InvC);
            if (_header_datetime == null)
            {
                throw new ArgumentNullException("DateTime converstion failed.");
            }

            return Issuer + delimiter +
                Card_Programe + delimiter +
                CreatedDataTime + delimiter +
                Sequence_number + delimiter +
                Production_batch_reference;
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


        public string Issuer
        {
            get
            {
                return _issuer;
            }
        }
        public string Card_Programe
        {

            get
            {

                return _card_program;

            }
        }
        public DateTime CreatedDataTime
        {

            get
            {

                return _datetime;

            }
        }
        public string Production_batch_reference
        {

            get
            {

                return _production_batch_reference;

            }
        }
        public string Sequence_number
        {

            get
            {

                return _sequence_number;

            }
        }

    }
}
