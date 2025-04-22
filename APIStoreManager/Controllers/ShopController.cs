using ApiStoreManager.Data;
using APIStoreManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIStoreManager.Controllers
{
    
    [Route("api/[Controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ShopController(ApplicationDbContext db) => _db = db;
        
        [HttpPost("CreateShop")]
        [Authorize]
        public async Task<IActionResult> CreateShop([FromBody] CreateShopDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var shop = new Shop
            {
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = userId
            };
            _db.Shops.Add(shop);
            await _db.SaveChangesAsync();
            return Ok(shop);
        }
        [HttpGet("MyShopsList")]
        [Authorize]
        public async Task<IActionResult> GetMyShops()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var myShops = await _db.Shops.Where(s => s.OwnerId == userId).ToListAsync();
            return Ok(myShops);
        }
        [HttpPost("{shopId}/CreateProduct")]
        [Authorize]
        public async Task<IActionResult> CreateProduct(int shopId, [FromBody] CreateProductDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var shop = await _db.Shops.FindAsync(shopId);
            if (shop == null)
            {
                return NotFound("Cửa hàng không tồn tại!");
            }

            if (shop.OwnerId != userId)
            {
                return Forbid();
            }

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                ShopId = shopId
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return Ok(product);
        }


        [HttpGet("{shopId}/products")]
        public async Task<IActionResult> GetProductsByShop(int shopId)
        {
            var shop = await _db.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == shopId);

            if (shop == null)
            {
                return NotFound("Cửa hàng không tồn tại!");
            }

            var result = new ShopAndProductsDto
            {
                ShopId = shop.Id,
                ShopName = shop.Name,
                ShopDescription = shop.Description,
                Products = shop.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price
                }).ToList()
            };

            return Ok(result);
        }
    }
}