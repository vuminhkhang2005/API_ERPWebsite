using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebERP.Models
{

    public class SalesOrder
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();


        public SalesOrder()
        {
            OrderDate = DateTime.Now;
            Status = "Pending";
            TotalAmount = 0;
        }

        public SalesOrder(int customerId)
        {
            CustomerId = customerId;
            OrderDate = DateTime.Now;
            Status = "Pending";
            TotalAmount = 0;
        }


        public void AddItem(Product product, int quantity)
        {
            var detail = new OrderDetail(this, product, quantity);
            OrderDetails.Add(detail);

            TotalAmount += product.UnitPrice * quantity;
        }

        public void Complete()
        {
            Status = "Completed";
        }

        public void Reject()
        {
            Status = "Rejected";
        }

        public void SetBackOrder()
        {
            Status = "Backorder";
        }
    }
}
