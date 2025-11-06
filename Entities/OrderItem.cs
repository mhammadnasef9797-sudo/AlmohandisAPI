namespace AlmohandisAPI.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // نحفظ الاسم هنا
        public decimal Price { get; set; } // نحفظ السعر هنا
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}