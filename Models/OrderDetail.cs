using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;

namespace WebERP.Models
{
    public class OrderDetail
    {
        [Key]
        public int DetailId {  get; set; }
        public int OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public SalesOrder? SalesOrder { get; set; }
        
        public int ProductId {  get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public OrderDetail()
        {

        }
        public OrderDetail(SalesOrder order, Product product, int quantity)
        {
            this.SalesOrder = order;
            this.Product = product;
            this.Quantity = quantity;
        }

        public void IncreaseQuantity()
        {
            this.Quantity ++;
        }
        public void DecreaseQuantity()
        {
            this.Quantity--;
            if (Quantity < 0) Quantity = 0;
        }
        public decimal GetTotalAmount()
        {
            return Quantity * UnitPrice;
        }
    }
}
