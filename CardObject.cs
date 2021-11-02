using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Veneka.Indigo.Integration.Fidelity
{
    class CardObject2
    {
        public string Bincode { get; set; }
        public string Uniquenumber { get; set; }
        public SqlConnection ConnectionString { get; set; }
        public int noofcards { get; set; }
        public int latestsequencenumber { get; set; }
        public int auditUserId { get; set; }
        public string BranchCode { get; set; }
        public string auditWorkStation { get; set; }
    }
}
