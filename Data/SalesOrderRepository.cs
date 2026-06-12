using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using WebERP.DTOs;
using WebERP.Models;

namespace WebERP.Data
{
    public class SalesOrderRepository
    {
        private readonly DbHelper db;

        public SalesOrderRepository(DbHelper db)
        {
            this.db = db;
        }

        public object GetSalesOrdersPaged(int page, int size, string searchTerm, int? customerId = null)
        {
            var list = new List<SalesOrder>();
            int totalRecords = 0;

            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_get_sales_orders_paged", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@page_number", page);
                cmd.Parameters.AddWithValue("@page_size", size);
                cmd.Parameters.AddWithValue("@search_term", searchTerm ?? "");
                cmd.Parameters.AddWithValue("@customer_id", (object)customerId ?? DBNull.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SalesOrder
                        {
                            OrderId = Convert.ToInt32(reader["orderid"]),
                            CustomerId = Convert.ToInt32(reader["customerid"]),
                            Customer = new Customer
                            {
                                customerId = Convert.ToInt32(reader["customerid"]),
                                customerName = reader["customername"].ToString()
                            },
                            OrderDate = Convert.ToDateTime(reader["orderdate"]),
                            Status = reader["status"].ToString(),
                            TotalAmount = Convert.ToDecimal(reader["totalamount"])
                        });

                        totalRecords = Convert.ToInt32(reader["totalcount"]);
                    }
                }
            }

            int lastPage = (int)Math.Ceiling((double)totalRecords / size);
            if (lastPage == 0) lastPage = 1;

            return new
            {
                last_page = lastPage,
                data = list
            };
        }

        public int AddSalesOrder(int customerId, string status)
        {
            int newOrderId = 0;
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_insert_sales_order", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@customerid", customerId);
                cmd.Parameters.AddWithValue("@orderdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@status", status);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        newOrderId = Convert.ToInt32(reader["neworderid"]);
                    }
                }
            }
            return newOrderId;
        }

        public void UpdateSalesOrderStatus(int orderId, string status)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_update_sales_order_status", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@orderid", orderId);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void UpdateSalesOrderCustomer(int orderId, int customerId)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE SalesOrders SET CustomerId = @customerid WHERE OrderId = @orderid", conn);
                cmd.Parameters.AddWithValue("@orderid", orderId);
                cmd.Parameters.AddWithValue("@customerid", customerId);
                cmd.ExecuteNonQuery();
            }
        }

        public void RemoveSalesOrder(int orderId)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM StockMovements WHERE OrderId = @orderid; DELETE FROM OrderDetails WHERE OrderId = @orderid; DELETE FROM SalesOrders WHERE OrderId = @orderid;", conn);
                cmd.Parameters.AddWithValue("@orderid", orderId);
                cmd.ExecuteNonQuery();
            }
        }

        public int SaveFullOrder(int orderId, int customerId, string status, System.Text.Json.Nodes.JsonArray details)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int finalOrderId = orderId;
                        if (orderId == 0)
                        {
                            var cmd = new SqlCommand("sp_insert_sales_order", conn, tx);
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@customerid", customerId);
                            cmd.Parameters.AddWithValue("@orderdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@status", status);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) finalOrderId = Convert.ToInt32(reader["neworderid"]);
                            }
                        }
                        else
                        {
                            var cmd1 = new SqlCommand("sp_update_sales_order_status", conn, tx);
                            cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd1.Parameters.AddWithValue("@orderid", orderId);
                            cmd1.Parameters.AddWithValue("@status", status);
                            cmd1.ExecuteNonQuery();

                            var cmd2 = new SqlCommand("UPDATE SalesOrders SET CustomerId = @customerid WHERE OrderId = @orderid", conn, tx);
                            cmd2.Parameters.AddWithValue("@orderid", orderId);
                            cmd2.Parameters.AddWithValue("@customerid", customerId);
                            cmd2.ExecuteNonQuery();

                            var cmd3 = new SqlCommand("DELETE FROM OrderDetails WHERE OrderId = @orderid", conn, tx);
                            cmd3.Parameters.AddWithValue("@orderid", orderId);
                            cmd3.ExecuteNonQuery();
                        }

                        if (details != null)
                        {
                            foreach (var itemNode in details)
                            {
                                var item = itemNode.AsObject();
                                int prodId = (int)item["productId"];
                                int qty = (int)item["quantity"];

                                var cmdD = new SqlCommand("sp_insert_order_detail", conn, tx);
                                cmdD.CommandType = System.Data.CommandType.StoredProcedure;
                                cmdD.Parameters.AddWithValue("@orderid", finalOrderId);
                                cmdD.Parameters.AddWithValue("@productid", prodId);
                                cmdD.Parameters.AddWithValue("@quantity", qty);
                                cmdD.ExecuteNonQuery();
                            }
                        }

                        var cmdTotal = new SqlCommand("UPDATE SalesOrders SET TotalAmount = ISNULL((SELECT SUM(Quantity * UnitPrice) FROM OrderDetails WHERE OrderDetails.OrderId = SalesOrders.OrderId), 0) WHERE OrderId = @orderid", conn, tx);
                        cmdTotal.Parameters.AddWithValue("@orderid", finalOrderId);
                        cmdTotal.ExecuteNonQuery();

                        tx.Commit();
                        return finalOrderId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void EnsureProductsAvailable(IEnumerable<OrderDetailInputDto> details)
        {
            var requestedItems = details
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            if (requestedItems.Count == 0)
            {
                throw new Exception("Order must contain at least one product.");
            }

            using (var conn = db.GetConnection())
            {
                conn.Open();
                foreach (var item in requestedItems)
                {
                    using (var cmd = new SqlCommand("SELECT ProductName, ISNULL(StockQty, 0) AS StockQty FROM Products WHERE ProductID = @productId", conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", item.ProductId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                throw new Exception($"Product #{item.ProductId} was not found.");
                            }

                            string productName = reader["ProductName"]?.ToString() ?? $"Product #{item.ProductId}";
                            int stockQty = Convert.ToInt32(reader["StockQty"]);
                            if (item.Quantity > stockQty)
                            {
                                throw new Exception($"{productName} has only {stockQty} unit(s) in stock.");
                            }
                        }
                    }
                }
            }
        }

        public List<Customer> GetAllCustomersForDropdown()
        {
            var list = new List<Customer>();
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT CustomerId, CustomerName FROM Customers ORDER BY CustomerName", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Customer
                        {
                            customerId = (int)reader["CustomerId"],
                            customerName = reader["CustomerName"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }
}
