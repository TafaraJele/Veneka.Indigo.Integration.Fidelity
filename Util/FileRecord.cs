using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veneka.Indigo.Integration.Fidelity.Util
{
    public abstract class FileRecord
    {
        public abstract String OutputLine();
        public abstract string CheckForNull(string field);
    }
}
