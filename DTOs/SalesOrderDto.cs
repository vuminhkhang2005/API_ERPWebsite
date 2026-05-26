using System.Collections.Generic;

namespace WebERP.DTOs
{
    public class SalesOrderCreateDto
    {
        public int CustomerId { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class SalesOrderUpdateDto
    {
        public int CustomerId { get; set; }
        public string Status { get; set; }
    }

    public class SaveFullOrderDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string Status { get; set; }
        public List<OrderDetailInputDto> Details { get; set; } = new();
    }

    public class OrderDetailInputDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
