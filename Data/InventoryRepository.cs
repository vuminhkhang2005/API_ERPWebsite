using Microsoft.Data.SqlClient;
using System.Data;

namespace WebERP.Data
{
    public class InventoryRepository
    {
        private readonly DbHelper db;

        public InventoryRepository(DbHelper db)
        {
            this.db = db;
        }

        public object GetProductsPaged(int page, int size, string searchTerm)
        {
            var list = new List<object>();
            int totalRecords = 0;
            int offset = (Math.Max(page, 1) - 1) * Math.Max(size, 1);

            using (var conn = db.GetConnection())
            {
                conn.Open();
                string where = @"
                    WHERE (@search = ''
                        OR p.ProductName LIKE '%' + @search + '%'
                        OR c.CategoryName LIKE '%' + @search + '%'
                        OR CAST(p.ProductID AS NVARCHAR(20)) = @search)";

                using (var countCmd = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                    " + where, conn))
                {
                    countCmd.Parameters.AddWithValue("@search", searchTerm ?? "");
                    totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                using (var cmd = new SqlCommand(@"
                    SELECT
                        p.ProductID,
                        p.ProductName,
                        p.CategoryID,
                        c.CategoryName,
                        ISNULL(p.StockQty, 0) AS StockQty,
                        ISNULL(p.UnitPrice, 0) AS UnitPrice,
                        CAST(ISNULL(p.StockQty, 0) * ISNULL(p.UnitPrice, 0) AS DECIMAL(18,2)) AS StockValue
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                    " + where + @"
                    ORDER BY p.ProductID
                    OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY;", conn))
                {
                    cmd.Parameters.AddWithValue("@search", searchTerm ?? "");
                    cmd.Parameters.AddWithValue("@offset", offset);
                    cmd.Parameters.AddWithValue("@size", Math.Max(size, 1));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new
                            {
                                productId = Convert.ToInt32(reader["ProductID"]),
                                productName = reader["ProductName"]?.ToString() ?? "",
                                categoryId = reader["CategoryID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CategoryID"]),
                                categoryName = reader["CategoryName"]?.ToString() ?? "",
                                stockQty = Convert.ToInt32(reader["StockQty"]),
                                unitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                stockValue = Convert.ToDecimal(reader["StockValue"])
                            });
                        }
                    }
                }
            }

            int lastPage = (int)Math.Ceiling((double)totalRecords / Math.Max(size, 1));
            if (lastPage == 0) lastPage = 1;

            return new
            {
                last_page = lastPage,
                data = list
            };
        }

        public object GetMovementsPaged(int page, int size, string searchTerm)
        {
            var list = new List<object>();
            int totalRecords = 0;
            int offset = (Math.Max(page, 1) - 1) * Math.Max(size, 1);

            using (var conn = db.GetConnection())
            {
                conn.Open();
                string where = @"
                    WHERE (@search = ''
                        OR p.ProductName LIKE '%' + @search + '%'
                        OR w.WarehouseName LIKE '%' + @search + '%'
                        OR sm.MovementType LIKE '%' + @search + '%'
                        OR CAST(sm.MovementID AS NVARCHAR(20)) = @search
                        OR CAST(sm.OrderID AS NVARCHAR(20)) = @search)";

                using (var countCmd = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM StockMovements sm
                    LEFT JOIN Products p ON sm.ProductID = p.ProductID
                    LEFT JOIN Warehouses w ON sm.WarehouseID = w.WarehouseID
                    " + where, conn))
                {
                    countCmd.Parameters.AddWithValue("@search", searchTerm ?? "");
                    totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                using (var cmd = new SqlCommand(@"
                    SELECT
                        sm.MovementID,
                        sm.ProductID,
                        p.ProductName,
                        sm.WarehouseID,
                        w.WarehouseName,
                        sm.MovementType,
                        sm.Quantity,
                        sm.MovementDate,
                        sm.OrderID
                    FROM StockMovements sm
                    LEFT JOIN Products p ON sm.ProductID = p.ProductID
                    LEFT JOIN Warehouses w ON sm.WarehouseID = w.WarehouseID
                    " + where + @"
                    ORDER BY sm.MovementDate DESC, sm.MovementID DESC
                    OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY;", conn))
                {
                    cmd.Parameters.AddWithValue("@search", searchTerm ?? "");
                    cmd.Parameters.AddWithValue("@offset", offset);
                    cmd.Parameters.AddWithValue("@size", Math.Max(size, 1));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new
                            {
                                movementId = Convert.ToInt32(reader["MovementID"]),
                                productId = reader["ProductID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProductID"]),
                                productName = reader["ProductName"]?.ToString() ?? "",
                                warehouseId = reader["WarehouseID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["WarehouseID"]),
                                warehouseName = reader["WarehouseName"]?.ToString() ?? "",
                                movementType = reader["MovementType"]?.ToString() ?? "",
                                quantity = reader["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Quantity"]),
                                movementDate = reader["MovementDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["MovementDate"]),
                                orderId = reader["OrderID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["OrderID"])
                            });
                        }
                    }
                }
            }

            int lastPage = (int)Math.Ceiling((double)totalRecords / Math.Max(size, 1));
            if (lastPage == 0) lastPage = 1;

            return new
            {
                last_page = lastPage,
                data = list
            };
        }

        public List<object> GetCategories()
        {
            var list = new List<object>();
            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            categoryId = Convert.ToInt32(reader["CategoryID"]),
                            categoryName = reader["CategoryName"]?.ToString() ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public List<object> GetWarehouses()
        {
            var list = new List<object>();
            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT WarehouseID, WarehouseName, Location FROM Warehouses ORDER BY WarehouseID", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            warehouseId = Convert.ToInt32(reader["WarehouseID"]),
                            warehouseName = reader["WarehouseName"]?.ToString() ?? "",
                            location = reader["Location"]?.ToString() ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public int AddProduct(string productName, int categoryId, decimal unitPrice)
        {
            ValidateProduct(productName, categoryId, unitPrice);

            using (var conn = db.GetConnection())
            {
                conn.Open();
                EnsureCategoryExists(conn, null, categoryId);

                using (var tx = conn.BeginTransaction())
                using (var cmd = new SqlCommand(@"
                    DECLARE @newId INT;
                    SELECT @newId = ISNULL(MAX(ProductID), 0) + 1 FROM Products WITH (UPDLOCK, HOLDLOCK);

                    INSERT INTO Products (ProductID, ProductName, CategoryID, StockQty, UnitPrice)
                    VALUES (@newId, @productName, @categoryId, 0, @unitPrice);

                    SELECT @newId;", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@productName", productName.Trim());
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);
                    cmd.Parameters.AddWithValue("@unitPrice", unitPrice);
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    tx.Commit();
                    return newId;
                }
            }
        }

        public void UpdateProduct(int productId, string productName, int categoryId, decimal unitPrice)
        {
            ValidateProduct(productName, categoryId, unitPrice);

            using (var conn = db.GetConnection())
            {
                conn.Open();
                EnsureCategoryExists(conn, null, categoryId);

                using (var cmd = new SqlCommand(@"
                    UPDATE Products
                    SET ProductName = @productName,
                        CategoryID = @categoryId,
                        UnitPrice = @unitPrice
                    WHERE ProductID = @productId;", conn))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    cmd.Parameters.AddWithValue("@productName", productName.Trim());
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);
                    cmd.Parameters.AddWithValue("@unitPrice", unitPrice);

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        throw new Exception("Product not found.");
                    }
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                int references = Convert.ToInt32(new SqlCommand(@"
                    SELECT
                        (SELECT COUNT(*) FROM OrderDetails WHERE ProductID = @productId) +
                        (SELECT COUNT(*) FROM StockMovements WHERE ProductID = @productId);", conn)
                {
                    Parameters = { new SqlParameter("@productId", productId) }
                }.ExecuteScalar());

                if (references > 0)
                {
                    throw new Exception("Product has order details or stock movements and cannot be deleted.");
                }

                using (var cmd = new SqlCommand("DELETE FROM Products WHERE ProductID = @productId", conn))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        throw new Exception("Product not found.");
                    }
                }
            }
        }

        public int AddWarehouse(string warehouseName, string location)
        {
            ValidateWarehouse(warehouseName, location);

            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                using (var cmd = new SqlCommand(@"
                    DECLARE @newId INT;
                    SELECT @newId = ISNULL(MAX(WarehouseID), 0) + 1 FROM Warehouses WITH (UPDLOCK, HOLDLOCK);

                    INSERT INTO Warehouses (WarehouseID, WarehouseName, Location)
                    VALUES (@newId, @warehouseName, @location);

                    SELECT @newId;", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@warehouseName", warehouseName.Trim());
                    cmd.Parameters.AddWithValue("@location", location.Trim());
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    tx.Commit();
                    return newId;
                }
            }
        }

        public void UpdateWarehouse(int warehouseId, string warehouseName, string location)
        {
            ValidateWarehouse(warehouseName, location);

            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Warehouses
                    SET WarehouseName = @warehouseName,
                        Location = @location
                    WHERE WarehouseID = @warehouseId;", conn))
                {
                    cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
                    cmd.Parameters.AddWithValue("@warehouseName", warehouseName.Trim());
                    cmd.Parameters.AddWithValue("@location", location.Trim());

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        throw new Exception("Warehouse not found.");
                    }
                }
            }
        }

        public void DeleteWarehouse(int warehouseId)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                int movements = Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM StockMovements WHERE WarehouseID = @warehouseId", conn)
                {
                    Parameters = { new SqlParameter("@warehouseId", warehouseId) }
                }.ExecuteScalar());

                if (movements > 0)
                {
                    throw new Exception("Warehouse has stock movements and cannot be deleted.");
                }

                using (var cmd = new SqlCommand("DELETE FROM Warehouses WHERE WarehouseID = @warehouseId", conn))
                {
                    cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        throw new Exception("Warehouse not found.");
                    }
                }
            }
        }

        public int AddStockMovement(int productId, int warehouseId, string movementType, int quantity)
        {
            string normalizedType = NormalizeMovementType(movementType);
            if (quantity <= 0)
            {
                throw new Exception("Quantity must be greater than zero.");
            }

            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    EnsureProductExists(conn, tx, productId);
                    EnsureWarehouseExists(conn, tx, warehouseId);

                    int currentStock = GetCurrentStock(conn, tx, productId);
                    if (normalizedType == "OUT" && currentStock < quantity)
                    {
                        throw new Exception($"Insufficient stock. Current stock is {currentStock}.");
                    }

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO StockMovements (ProductID, WarehouseID, MovementType, Quantity, MovementDate, OrderID)
                        OUTPUT INSERTED.MovementID
                        VALUES (@productId, @warehouseId, @movementType, @quantity, GETDATE(), NULL);", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
                        cmd.Parameters.AddWithValue("@movementType", normalizedType);
                        cmd.Parameters.AddWithValue("@quantity", quantity);

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        tx.Commit();
                        return newId;
                    }
                }
            }
        }

        public void DeleteStockMovement(int movementId)
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    int? orderId = null;
                    using (var checkCmd = new SqlCommand("SELECT OrderID FROM StockMovements WHERE MovementID = @movementId", conn, tx))
                    {
                        checkCmd.Parameters.AddWithValue("@movementId", movementId);
                        var result = checkCmd.ExecuteScalar();
                        if (result == null)
                        {
                            throw new Exception("Stock movement not found.");
                        }
                        orderId = result == DBNull.Value ? null : (int?)Convert.ToInt32(result);
                    }

                    if (orderId.HasValue)
                    {
                        throw new Exception("Order-generated stock movements cannot be deleted manually.");
                    }

                    using (var cmd = new SqlCommand("DELETE FROM StockMovements WHERE MovementID = @movementId", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@movementId", movementId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        private static void ValidateProduct(string productName, int categoryId, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new Exception("Product name is required.");
            }
            if (categoryId <= 0)
            {
                throw new Exception("Category is required.");
            }
            if (unitPrice < 0)
            {
                throw new Exception("Unit price must be greater than or equal to zero.");
            }
        }

        private static void ValidateWarehouse(string warehouseName, string location)
        {
            if (string.IsNullOrWhiteSpace(warehouseName))
            {
                throw new Exception("Warehouse name is required.");
            }
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new Exception("Location is required.");
            }
        }

        private static string NormalizeMovementType(string movementType)
        {
            string normalized = (movementType ?? "").Trim().ToUpperInvariant();
            if (normalized != "IN" && normalized != "OUT")
            {
                throw new Exception("Movement type must be IN or OUT.");
            }

            return normalized;
        }

        private static void EnsureCategoryExists(SqlConnection conn, SqlTransaction? tx, int categoryId)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Categories WHERE CategoryID = @categoryId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@categoryId", categoryId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    throw new Exception("Category not found.");
                }
            }
        }

        private static void EnsureProductExists(SqlConnection conn, SqlTransaction tx, int productId)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Products WITH (UPDLOCK, HOLDLOCK) WHERE ProductID = @productId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    throw new Exception("Product not found.");
                }
            }
        }

        private static void EnsureWarehouseExists(SqlConnection conn, SqlTransaction tx, int warehouseId)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Warehouses WHERE WarehouseID = @warehouseId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    throw new Exception("Warehouse not found.");
                }
            }
        }

        private static int GetCurrentStock(SqlConnection conn, SqlTransaction tx, int productId)
        {
            using (var cmd = new SqlCommand("SELECT ISNULL(StockQty, 0) FROM Products WITH (UPDLOCK, HOLDLOCK) WHERE ProductID = @productId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
