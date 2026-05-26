using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebERP.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        public int StockQty { get; set; }
        public decimal UnitPrice { get; set; }


        public Product() { }
        public Product(string productName, int categoryId, int stockQty, decimal unitPrice)
        {
            ProductName = productName;
            CategoryId = categoryId;
            StockQty = stockQty;
            UnitPrice = unitPrice;
        }
        public void IncreaseStock(int quantity)
        {
            StockQty += quantity;
        }
        public void DecreaseStock(int quantity)
        {
            if (StockQty < quantity)
                throw new Exception("Not enough stock");

            StockQty -= quantity;
        }
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new Exception("Price must be >= 0");

            UnitPrice = newPrice;
        }
    }
}
