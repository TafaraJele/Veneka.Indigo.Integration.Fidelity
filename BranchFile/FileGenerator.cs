using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veneka.Indigo.Integration.Fidelity.BranchFile.Objects;
using Veneka.Indigo.Integration.Fidelity.Util;

namespace Veneka.Indigo.Integration.Fidelity.BranchFile
{
    class FileGenerator
    {
        FileWriter writer = new FileWriter();

        public bool CreateBranchFile(string filename, HeaderRecord headerRecord, List<DataRecord> dataRecords, TrailerRecord trailerRecord, string outputDirectory)
        {
            List<FileRecord> fileRecords = new List<FileRecord>();

            //Add the header record first.
            fileRecords.Add(headerRecord);

            //Now add the data records
            foreach (var cr in dataRecords)
            {
                fileRecords.Add(cr);
            }

            //Add the trailer record first.
            fileRecords.Add(trailerRecord);

            writer.WriteFile(outputDirectory, filename.ToString(), fileRecords);

            return true;
        }
    }

}
