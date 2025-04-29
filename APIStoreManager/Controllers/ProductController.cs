using APIStoreManager.Model;
using APIStoreManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using ApiStoreManager.Data;
using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet("MyProductsList/{shopId}")]
        [Authorize]
        public async Task<IActionResult> GetMyShopProducts(int shopId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var shop = await _db.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == shopId && s.OwnerId == userId);

            if (shop == null)
            {
                return NotFound("Cửa hàng không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
            }

            var products = shop.Products.Select(p => new
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price
            }).ToList();

            return Ok(new
            {
                ShopId = shop.Id,
                ShopName = shop.Name,
                ShopDescription = shop.Description,
                Products = products
            });
        }


        [HttpPost("{shopId}/CreateProduct")]
        [Authorize]
        public async Task<IActionResult> CreateProduct(int shopId, [FromBody] ProductDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var shop = await _db.Shops.FindAsync(shopId);
            if (shop == null) return NotFound("Cửa hàng không tồn tại!");
            if (shop.OwnerId != userId) return Forbid();

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


        [HttpPut("{productId}/UpdateProduct")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductDto updatedProduct)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var product = await _db.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId && p.Shop.OwnerId == userId);

            if (product == null)
                return NotFound("Sản phẩm không tồn tại hoặc bạn không có quyền!");

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật Sản phẩm thành công!",
                data = product
            });
        }

        
        [HttpDelete("{productId}/DeleteProduct")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var product = await _db.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId && p.Shop.OwnerId == userId);

            if (product == null)
                return NotFound("Sản phẩm không tồn tại hoặc bạn không có quyền!");

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return Ok("Đã xóa sản phẩm.");
        }
    }
}