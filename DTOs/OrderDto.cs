namespace AlmohandisAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}