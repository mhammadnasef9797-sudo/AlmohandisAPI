using AlmohandisAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlmohandisAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; } // <-- 
        public DbSet<CartItem> CartItems { get; set; } // <-- 
        public DbSet<Order> Orders { get; set; } // <-- أضف هذا
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
    }
}