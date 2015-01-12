using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    class SqlGateway
    {
        private readonly  string _connectionString;

        public SqlGateway(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void RunQuery(string query)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetInt(string query)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = query;
                    var result = cmd.ExecuteScalar();
                    return (int)result;
                }
            }
        }
    }
}
