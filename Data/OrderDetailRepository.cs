using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using WebERP.Models;

namespace WebERP.Data
{
    public class OrderDetailRepository
    {
        private readonly DbHelper db;

        public OrderDetailRepository(DbHelper db)
        {
            this.db = db;
        }

        public List<OrderDetail> GetOrderDetails(int orderId)
        {
            var list = new List<OrderDetail>();

            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_get_order_details", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@orderid", orderId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new OrderDetail
                        {
                            DetailId = Convert.ToInt32(reader["detailid"]),
                            OrderId = Convert.ToInt32(reader["orderid"]),
                            ProductId = Convert.ToInt32(reader["productid"]),
                            Product = new Product
                            {
                                ProductId = Convert.ToInt32(reader["productid"]),
                                ProductName = reader["productname"].ToString(),
                                UnitPrice = Convert.ToDecimal(reader["unitprice"])
                            },
                            Quantity = Convert.ToInt32(reader["quantity"]),
                            UnitPrice = Convert.ToDecimal(reader["unitprice"])
                        });
                    }
                }
            }
            // Tabulator sub-table ko cần phân trang
            return list;
        }

        public void AddOrderDetail(int orderId, int productId, int quantity)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_insert_order_detail", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@orderid", orderId);
                cmd.Parameters.AddWithValue("@productid", productId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateOrderDetailQuantity(int detailId, int quantity)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    DECLARE @orderid INT;
                    SELECT @orderid = orderid FROM OrderDetails WHERE DetailId = @detailid;
                    UPDATE OrderDetails SET Quantity = @quantity WHERE DetailId = @detailid;
                    UPDATE SalesOrders SET TotalAmount = ISNULL((SELECT SUM(Quantity * UnitPrice)
                    FROM OrderDetails WHERE OrderId = @orderid), 0) WHERE OrderId = @orderid;
                ", conn);
                cmd.Parameters.AddWithValue("@detailid", detailId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.ExecuteNonQuery();
            }
        }

        public void RemoveOrderDetail(int detailId)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("sp_delete_order_detail", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@detailid", detailId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Product> GetAllProductsForDropdown()
        {
            var list = new List<Product>();
            using (var conn = db.GetConnection())
            {
                conn.Open();
                // Dùng Raw SQL để gọi trực tiếp Function fn_get_stock_qty
                var cmd = new SqlCommand("SELECT ProductId, ProductName, UnitPrice, dbo.fn_get_stock_qty(ProductId) AS StockQty FROM Products ORDER BY ProductName", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Product
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            ProductName = reader["ProductName"].ToString(),
                            UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                            StockQty = Convert.ToInt32(reader["StockQty"])
                        });
                    }
                }
            }
            return list;
        }
    }
}
