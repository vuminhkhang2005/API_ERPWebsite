using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using WebERP.Helpers;

namespace WebERP.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DbHelper>();

                using (var conn = db.GetConnection())
                {
                    conn.Open();

                    // 1. Create Users Table if not exists
                    string createUsersTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                        BEGIN
                            CREATE TABLE Users (
                                UserId INT IDENTITY(1,1) PRIMARY KEY,
                                Username NVARCHAR(50) NOT NULL UNIQUE,
                                PasswordHash NVARCHAR(255) NOT NULL,
                                Role NVARCHAR(20) NOT NULL,
                                CustomerId INT NULL FOREIGN KEY REFERENCES Customers(CustomerId)
                            );
                        END";
                    using (var cmd = new SqlCommand(createUsersTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Seed default users if table is empty
                    string checkUsersSql = "SELECT COUNT(*) FROM Users";
                    int userCount = 0;
                    using (var cmd = new SqlCommand(checkUsersSql, conn))
                    {
                        userCount = (int)cmd.ExecuteScalar();
                    }

                    if (userCount == 0)
                    {
                        string adminHash = PasswordHelper.HashPassword("admin123");
                        string employeeHash = PasswordHelper.HashPassword("employee123");
                        string customerHash = PasswordHelper.HashPassword("customer123");

                        // Check if CustomerId = 1 exists in the database
                        string checkCustomerSql = "SELECT COUNT(*) FROM Customers WHERE CustomerId = 1";
                        int customerExists = 0;
                        using (var cmd = new SqlCommand(checkCustomerSql, conn))
                        {
                            customerExists = (int)cmd.ExecuteScalar();
                        }

                        string insertSql = @"
                            INSERT INTO Users (Username, PasswordHash, Role, CustomerId) 
                            VALUES (@u1, @p1, @r1, NULL);

                            INSERT INTO Users (Username, PasswordHash, Role, CustomerId) 
                            VALUES (@u2, @p2, @r2, NULL);";

                        if (customerExists > 0)
                        {
                            insertSql += @"
                                INSERT INTO Users (Username, PasswordHash, Role, CustomerId) 
                                VALUES (@u3, @p3, @r3, 1);";
                        }
                        else
                        {
                            insertSql += @"
                                INSERT INTO Users (Username, PasswordHash, Role, CustomerId) 
                                VALUES (@u3, @p3, @r3, NULL);";
                        }

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@u1", "admin");
                            cmd.Parameters.AddWithValue("@p1", adminHash);
                            cmd.Parameters.AddWithValue("@r1", "Admin");

                            cmd.Parameters.AddWithValue("@u2", "employee");
                            cmd.Parameters.AddWithValue("@p2", employeeHash);
                            cmd.Parameters.AddWithValue("@r2", "Employee");

                            cmd.Parameters.AddWithValue("@u3", "customer1");
                            cmd.Parameters.AddWithValue("@p3", customerHash);
                            cmd.Parameters.AddWithValue("@r3", "Customer");

                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Alter sp_get_sales_orders_paged stored procedure to support optional @customer_id filtering
                    string alterStoredProcSql = @"
                        CREATE OR ALTER PROCEDURE sp_get_sales_orders_paged
                            @page_number INT,
                            @page_size INT,
                            @search_term NVARCHAR(100),
                            @customer_id INT = NULL
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            DECLARE @offset INT = (@page_number - 1) * @page_size;
                            DECLARE @total_records INT;

                            SELECT @total_records = COUNT(*)
                            FROM SalesOrders so
                            JOIN Customers c ON so.CustomerId = c.CustomerId
                            WHERE (@customer_id IS NULL OR so.CustomerId = @customer_id)
                              AND (@search_term = '' 
                                   OR c.CustomerName LIKE '%' + @search_term + '%'
                                   OR CAST(so.OrderId AS NVARCHAR) = @search_term
                                   OR so.Status LIKE '%' + @search_term + '%');

                            SELECT 
                                so.OrderId,
                                so.CustomerId,
                                c.CustomerName,
                                so.OrderDate,
                                so.Status,
                                so.TotalAmount,
                                @total_records AS TotalCount
                            FROM SalesOrders so
                            JOIN Customers c ON so.CustomerId = c.CustomerId
                            WHERE (@customer_id IS NULL OR so.CustomerId = @customer_id)
                              AND (@search_term = '' 
                                   OR c.CustomerName LIKE '%' + @search_term + '%'
                                   OR CAST(so.OrderId AS NVARCHAR) = @search_term
                                   OR so.Status LIKE '%' + @search_term + '%')
                            ORDER BY so.OrderId ASC
                            OFFSET @offset ROWS
                            FETCH NEXT @page_size ROWS ONLY;
                        END;";
                    using (var cmd = new SqlCommand(alterStoredProcSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
