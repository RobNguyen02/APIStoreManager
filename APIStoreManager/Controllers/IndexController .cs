using APIStoreManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.Controllers
{
    [Route("api")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly StoreManagerContext _db;

        public IndexController(StoreManagerContext db)
        {
            _db = db;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] double? minPrice = null,
            [FromQuery] double? maxPrice = null)
        {
            var query = _db.Products.Include(p => p.Shop).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.Name.Contains(keyword));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var total = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    shopName = p.Shop.Name
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                products
            });
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var product = await _db.Products.Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Không tìm thấy sản phẩm");

            return Ok(new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                shop = new
                {
                    id = product.Shop.Id,
                    name = product.Shop.Name
                }
            });
        }

        [HttpGet("shops")]
        public async Task<IActionResult> GetShops(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null)
        {
            var query = _db.Shops.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(s => s.Name.Contains(keyword));

            var total = await query.CountAsync();

            var shops = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    description = s.Description
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                shops
            });
        }

        // GET: api/shops/{id}
        [HttpGet("shops/{id}")]
        public async Task<IActionResult> GetShopDetail(int id)
        {
            var shop = await _db.Shops.Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shop == null)
                return NotFound("Không tìm thấy shop");

            return Ok(new
            {
                id = shop.Id,
                name = shop.Name,
                description = shop.Description,
                products = shop.Products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price
                })
            });
        }
    }
}
