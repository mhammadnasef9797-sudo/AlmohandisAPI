using System;
using System.Collections.Generic;

namespace AlmohandisAPI.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; } = "Pending"; // (قيد الانتظار, شحن, تم التوصيل)
        public List<OrderItem> Items { get; set; } = new();
    }
}