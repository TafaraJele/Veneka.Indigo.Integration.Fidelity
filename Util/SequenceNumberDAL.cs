using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Veneka.Indigo.Integration.Fidelity.Util
{
    class SequenceNumberDAL
    {
        private string connectionString;
        public enum ResetPeriod { DAILY = 1, WEEKLY = 2, MONTHLY = 3, YEARLY = 4 };

        public SequenceNumberDAL(string ConnectionString)
        {
            this.connectionString = ConnectionString;
        }

        /// <summary>
        /// Fetch the next sequence number to be used. This method is thread-safe.
        /// </summary>
        /// <param name="sequenceName">Name of the sequence</param>
        /// <param name="resetPeriod">How often this sequence is reset.</param>
        /// <returns></returns>
        public int NextSequenceNumber(string sequenceName, ResetPeriod resetPeriod)
        {
            //Fetch latest sequence number for transaction. 
            //Should be stored proc which updates the sequence
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[usp_get_next_sequence]";

                    command.Parameters.Add("@sequence_name", SqlDbType.VarChar).Value = sequenceName;
                    command.Parameters.Add("@reset_period", SqlDbType.Int).Value = (int)resetPeriod;
                    var seqNumber = command.ExecuteScalar();

                    return int.Parse(seqNumber.ToString());
                }
            }
        }
    }
}
