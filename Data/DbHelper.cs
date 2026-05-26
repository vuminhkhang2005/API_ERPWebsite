using Microsoft.Data.SqlClient;
using System.Data;

namespace WebERP.Data
{
    public class DbHelper
    {
        private readonly string connectionString;
        public DbHelper(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}