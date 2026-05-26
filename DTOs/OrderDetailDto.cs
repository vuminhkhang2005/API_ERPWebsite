namespace WebERP.DTOs
{
    public class OrderDetailCreateDto
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderDetailUpdateDto
    {
        public int Quantity { get; set; }
    }
}
