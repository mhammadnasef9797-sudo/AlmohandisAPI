using AlmohandisAPI.Data;
using AlmohandisAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmohandisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // المسار سيكون /api/products
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/products  (لجلب كل المنتجات)
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET: api/products/5  (لجلب منتج واحد بالـ ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(); // إذا لم يتم العثور على المنتج
            }

            return Ok(product);
        }
    }
}