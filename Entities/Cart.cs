namespace AlmohandisAPI.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; } // لربط السلة بالمستخدم
        public User User { get; set; }
        public List<CartItem> Items { get; set; } = new(); // قائمة المنتجات في السلة
    }
}