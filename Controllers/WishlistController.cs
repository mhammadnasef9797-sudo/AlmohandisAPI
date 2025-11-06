using AlmohandisAPI.Data;
using AlmohandisAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AlmohandisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // كل وظائف المفضلة محمية
    public class WishlistController : ControllerBase
    {
        private readonly DataContext _context;

        public WishlistController(DataContext context)
        {
            _context = context;
        }

        // GET: api/wishlist (لجلب قائمة مفضلة المستخدم)
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetUserId();
            var wishlistItems = await _context.WishlistItems
                .Where(i => i.UserId == userId)
                .Include(i => i.Product) // لجلب معلومات المنتج
                .ToListAsync();
            
            return Ok(wishlistItems);
        }

        // POST: api/wishlist/{productId} (لإضافة منتج للمفضلة)
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = GetUserId();
            
            // التأكد من أن المنتج ليس في المفضلة بالفعل
            var exists = await _context.WishlistItems
                .AnyAsync(i => i.UserId == userId && i.ProductId == productId);

            if (exists) return BadRequest("المنتج موجود بالفعل في المفضلة.");

            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ProductId = productId
            };

            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok("تمت إضافة المنتج إلى المفضلة بنجاح.");
        }

        // DELETE: api/wishlist/{productId} (لحذف منتج من المفضلة)
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = GetUserId();
            var wishlistItem = await _context.WishlistItems
                .FirstOrDefaultAsync(i => i.UserId == userId && i.ProductId == productId);

            if (wishlistItem == null) return NotFound("المنتج غير موجود في المفضلة.");

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok("تم حذف المنتج من المفضلة.");
        }

        // دالة مساعدة لجلب ID المستخدم من التوكن
        private int GetUserId()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            return user.Id;
        }
    }
}