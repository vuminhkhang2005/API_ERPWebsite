using Microsoft.Data.SqlClient;
using WebERP.Models;

namespace WebERP.Data
{
    public class CustomerRepository
    {
        private readonly DbHelper db;
        public CustomerRepository(DbHelper db)
        {
            this.db = db;
        }
        public List<Customer> GetAll()
        {
            var list = new List<Customer>();

            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Customers", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Customer(
                        (int)reader["CustomerID"],
                        reader["CustomerName"].ToString(),
                        reader["City"].ToString(),
                        reader["CustomerType"].ToString()
                    ));
                }
            }
            return list;
        }
        public void AddCustomer(int id, string name, string city, string type)
        {

            using (var conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_InsertCustomer", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", id);
                    cmd.Parameters.AddWithValue("@CustomerName", name);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@CustomerType", type);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while trying to add the customer.", ex);
                }
            }
        }
        public void UpdateCustomer(int id, string name, string city, string type)
        {

            using (var conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_UpdateCustomer", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", id);
                    cmd.Parameters.AddWithValue("@CustomerName", name);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@CustomerType", type);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while trying to update customer information.", ex);
                }
            }
        }
        public Customer FindById(int id)
        {
            Customer result = null;

            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_GetCustomerById", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerID", id);
                using(var reader= cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new Customer(
                            (int)reader["CustomerID"],
                            reader["CustomerName"].ToString(),
                            reader["City"].ToString(),
                            reader["CustomerType"].ToString()
                        );
                    }
                }
            }
            return result;
        }
        public List<Customer> Find(string find)
        {
            var result = new List<Customer> ();

            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_FindCustomer", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Find",find);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Customer(
                            (int)reader["CustomerID"],
                            reader["CustomerName"].ToString(),
                            reader["City"].ToString(),
                            reader["CustomerType"].ToString()
                        ));
                    }
                }
            }
            return result;
        }

        public void RemoveCustomer(int id)
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_DeleteCustomer", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while trying to remove the customer.", ex);
            }
        }

        public object GetCustomersPaged(int page, int size, string searchTerm)
        {
            var list = new List<Customer>();
            int totalRecords = 0;

            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_GetCustomersPaged", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PageNumber", page);
                cmd.Parameters.AddWithValue("@PageSize", size);
                cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Customer(
                            (int)reader["CustomerID"],
                            reader["CustomerName"].ToString(),
                            reader["City"].ToString(),
                            reader["CustomerType"].ToString()
                        ));
                        
                        // Lấy tổng số record từ cột TotalCount (hoặc TotalRecords) do SP trả về
                        totalRecords = (int)reader["TotalCount"]; 
                    }
                }
            }

            // Tính toán tổng số trang cho Tabulator
            int lastPage = (int)Math.Ceiling((double)totalRecords / size);
            if (lastPage == 0) lastPage = 1;

            return new {
                last_page = lastPage,
                data = list
            };
        }
    }
}
