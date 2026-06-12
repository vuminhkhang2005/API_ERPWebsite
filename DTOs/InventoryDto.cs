namespace WebERP.DTOs
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ProductUpdateDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class WarehouseCreateDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class WarehouseUpdateDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class StockMovementCreateDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
