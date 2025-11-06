using AlmohandisAPI.Data;
using AlmohandisAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AlmohandisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly DataContext _context;

        public OrdersController(DataContext context)
        {
            _context = context;
        }

        // POST: api/orders (لإنشاء طلب جديد)
        [HttpPost]
        public async Task<IActionResult> CreateOrder(string shippingAddress)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // 1. جلب سلة المستخدم مع المنتجات بداخلها
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("سلة التسوق فارغة.");
            }

            // 2. إنشاء الطلب الجديد
            var order = new Order
            {
                UserId = user.Id,
                ShippingAddress = shippingAddress,
                TotalPrice = cart.Items.Sum(item => item.Product.Price * item.Quantity)
            };

            // 3. تحويل عناصر السلة إلى عناصر طلب
            foreach (var cartItem in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    Price = cartItem.Product.Price,
                    Quantity = cartItem.Quantity
                });
            }
            
            _context.Orders.Add(order);

            // 4. تفريغ سلة التسوق
            _context.CartItems.RemoveRange(cart.Items);
            
            await _context.SaveChangesAsync();
            
            return Ok(new { OrderId = order.Id });
        }

        // GET: api/orders (لجلب تاريخ طلبات المستخدم)
        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetUserOrders()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }
    }
}