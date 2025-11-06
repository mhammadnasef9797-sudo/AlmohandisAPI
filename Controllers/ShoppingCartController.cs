using AlmohandisAPI.Data;
using AlmohandisAPI.DTOs;
using AlmohandisAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Added for List
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AlmohandisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingCartController : ControllerBase
    {
        private readonly DataContext _context;

        public ShoppingCartController(DataContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var cart = await RetrieveCart();
            if (cart == null) return Ok(new CartDto { Id = 0, Items = new List<CartItemDto>() });
            
            var cartDto = MapCartToDto(cart); // Use the helper function
            return Ok(cartDto);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart(int productId, int quantity)
        {
            var cart = await RetrieveCart() ?? await CreateCart();
            if (cart == null) return BadRequest("لا يمكن إنشاء السلة.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound("المنتج غير موجود");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) item.Quantity += quantity;
            else cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok(MapCartToDto(cart));
            
            return BadRequest("فشل في إضافة المنتج للسلة");
        }
        
        // ▼▼▼ هذه هي الدالة الجديدة والمهمة ▼▼▼
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, int quantity)
        {
            var cart = await RetrieveCart();
            if (cart == null) return NotFound("السلة غير موجودة.");

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null) return NotFound("المنتج غير موجود في السلة.");

            if (quantity <= 0) cart.Items.Remove(item);
            else item.Quantity = quantity;

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok(MapCartToDto(cart));

            return BadRequest("فشل في تحديث كمية المنتج.");
        }

        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItemFromCart(int cartItemId)
        {
            var cart = await RetrieveCart();
            if (cart == null) return NotFound("السلة غير موجودة");

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null) return NotFound("المنتج غير موجود في السلة");

            _context.CartItems.Remove(item); // Remove from the specific DbSet
            await _context.SaveChangesAsync();
            return Ok(MapCartToDto(cart)); // Return the updated cart
        }

        // --- Helper Functions ---

        private async Task<Cart> RetrieveCart()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.User.Email == userEmail);
        }

        private async Task<Cart> CreateCart()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return null;
            
            var cart = new Cart { UserId = user.Id };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        
        private CartDto MapCartToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(item => new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Price = item.Product.Price,
                    ImageUrl = item.Product.ImageUrl,
                    Quantity = item.Quantity
                }).ToList()
            };
        }
    }
}