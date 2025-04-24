using ApiStoreManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.Controllers
{
    [Route("api")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public IndexController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(
            [FromQuery] int? productId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? keyword = null, 
            [FromQuery] decimal? minPrice = null, 
            [FromQuery] decimal? maxPrice = null)
        {
            if (productId.HasValue)
            {
                var product = await _db.Products
                    .Include(p => p.Shop)
                    .FirstOrDefaultAsync(p => p.Id == productId.Value);

                if (product == null) return NotFound("Sản phẩm không tồn tại!");

                var detail = new
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Shop = new
                    {
                        Id = product.Shop.Id,
                        Name = product.Shop.Name,
                        Description = product.Shop.Description
                    }
                };

                return Ok(detail);
            }

            // Trường hợp danh sách sản phẩm mix từ tất cả shop
            var query = _db.Products
                .Include(p => p.Shop)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.Name.Contains(keyword));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var total = await query.CountAsync();
            var products = await query
                .OrderBy(p => Guid.NewGuid())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ShopName = p.Shop.Name
                })
                .ToListAsync();

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Products = products
            });
        }
        [HttpGet("Index/Shoplist")]
        public async Task<IActionResult> Index(
            [FromQuery] int? productId,
            [FromQuery] int? shopId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null
)
        {
            if (productId.HasValue)
            {
                // Trả chi tiết sản phẩm và shop
                var product = await _db.Products
                    .Include(p => p.Shop)
                    .FirstOrDefaultAsync(p => p.Id == productId.Value);

                if (product == null)
                    return NotFound("Không tìm thấy sản phẩm!");

                return Ok(new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    price = product.Price,
                    shop = new
                    {
                        id = product.Shop.Id,
                        name = product.Shop.Name,
                        description = product.Shop.Description
                    }
                });
            }

            if (shopId.HasValue)
            {
                // Trả chi tiết shop và các sản phẩm trong đó
                var shop = await _db.Shops
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == shopId.Value);

                if (shop == null)
                    return NotFound("Không tìm thấy shop!");

                return Ok(new
                {
                    shopId = shop.Id,
                    shopName = shop.Name,
                    shopDescription = shop.Description,
                    products = shop.Products.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        price = p.Price
                    }).ToList()
                });
            }

            // Trường hợp không có gì: trả danh sách tất cả các shop (có phân trang + tìm tên)
            var query = _db.Shops.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s => s.Name.Contains(keyword));
            }

            var total = await query.CountAsync();
            var shops = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                currentPage = page,
                pageSize,
                total,
                shops = shops.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    description = s.Description
                })
            });
        }

    }
}
