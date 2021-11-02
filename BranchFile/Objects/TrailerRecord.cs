using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veneka.Indigo.Integration.Fidelity.Util;

namespace Veneka.Indigo.Integration.Fidelity.BranchFile.Objects
{
    public class TrailerRecord : FileRecord
    {

        private string _recordIdentifier = "EOF";
        private int _numberofrecords;
        private string delimiter = ",";

        public TrailerRecord(string recordidentifier, int numberofrecords)
        {
            this._recordIdentifier = recordidentifier;
            this._numberofrecords = numberofrecords;
        }

        public override string OutputLine()
        {
            return
                     RecordIdentifier + delimiter +
                     NumberofRecords.ToString("000000");
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

        public override string CheckForNull(string field)
        {
            throw new NotImplementedException();
        }

        public string RecordIdentifier
        {

            get
            {

                return _recordIdentifier;

            }
        }

        public int NumberofRecords
        {

            get
            {

                return _numberofrecords;

            }
        }
    }
}
