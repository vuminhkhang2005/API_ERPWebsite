namespace WebERP.DTOs
{
    public class CustomerCreateDto
    {
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string city { get; set; }
        public string customerType { get; set; }
    }

    public class CustomerUpdateDto
    {
        public string customerName { get; set; }
        public string city { get; set; }
        public string customerType { get; set; }
    }
}
