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

        [HttpPost("CreateProduct")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var shop = await _db.Shops.FindAsync(dto.ShopId);
            if (shop == null) return NotFound("Cửa hàng không tồn tại!");
            if (shop.OwnerId != userId) return Forbid();

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                ShopId = dto.ShopId
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("{productId}/UpdateProduct")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] CreateProductDto updatedProduct)
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
            return Ok(product);
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
            return Ok("Đã xóa sản phẩm!");
        }
    }
}