using AlmohandisAPI.Data;
using AlmohandisAPI.DTOs;
using AlmohandisAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List

namespace AlmohandisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;

        public AdminController(DataContext context)
        {
            _context = context;
        }

        // ========================
        // --- إدارة المنتجات ---
        // ========================

        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.ImageUrl = updatedProduct.ImageUrl;
            product.Category = updatedProduct.Category;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("products/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // ========================
        // --- إدارة الطلبات ---
        // ========================

        [HttpGet("orders")]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderDtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                UserEmail = order.User?.Email ?? "User Deleted",
                UserPhoneNumber = order.User?.PhoneNumber,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                TotalPrice = order.TotalPrice,
                OrderStatus = order.OrderStatus,
                Items = new List<OrderItemDto>() // Keep item list empty for the main view
            }).ToList();

            return Ok(orderDtos);
        }

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ هذه هي الدالة الجديدة ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        [HttpGet("orders/{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items) // Include items for detail view
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("الطلب غير موجود.");
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.User?.Email ?? "User Deleted",
                UserPhoneNumber = order.User?.PhoneNumber,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                TotalPrice = order.TotalPrice,
                OrderStatus = order.OrderStatus,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            return Ok(orderDto);
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ نهاية الدالة الجديدة ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.OrderStatus = status; 
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Order status updated successfully." });
        }

        // ========================
        // ---   إدارة المستخدمين  ---
        // ========================

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return await _context.Users
                .Select(u => new UserDto 
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("المستخدم غير موجود.");

            var currentAdminEmail = User.FindFirstValue(ClaimTypes.Email);
            if (user.Email.Equals(currentAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("لا يمكنك حذف حساب الأدمن الخاص بك.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}