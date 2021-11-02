using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Veneka.Indigo.Integration.Fidelity.BranchFile.Objects;

namespace Veneka.Indigo.Integration.Fidelity.Util
{
    public class FileWriter
    {
        public bool WriteFile(string outputDirectory, string fileName, List<FileRecord> records)
        {
            string filepath = Path.Combine(outputDirectory, fileName);

            if (String.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentNullException("File output directory parameter cannot be null or empty.");

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);


            if (File.Exists(filepath))
                throw new IOException(filepath + " Already Exists!");

            using (StreamWriter file = new StreamWriter(filepath))
            {
                foreach (FileRecord record in records)
                {
                    file.WriteLine(record.OutputLine());
                }
            }

            return true;
        }
    }
}
