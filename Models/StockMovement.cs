using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebERP.Models
{
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        
        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }

        public string MovementType { get; set; } // IN / OUT

        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; }

        // EF constructor
        private StockMovement() { }

        // Main constructor
        public StockMovement(Product product, Warehouse warehouse, string movementType, int quantity)
        {
            Product = product;
            Warehouse = warehouse;

            ProductId = product.ProductId;
            WarehouseId = warehouse.WarehouseId;

            MovementType = movementType;
            Quantity = quantity;
            MovementDate = DateTime.Now;
        }

        // ===== Business Logic =====

        public void Apply()
        {
            if (MovementType == "IN")
            {
                Product.IncreaseStock(Quantity);
            }
            else if (MovementType == "OUT")
            {
                Product.DecreaseStock(Quantity);
            }
            else
            {
                throw new Exception("Invalid movement type");
            }
        }
    }
}
