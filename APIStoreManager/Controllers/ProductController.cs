using APIStoreManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using APIStoreManager.Data;
using Microsoft.EntityFrameworkCore;
using APIStoreManager.DTOs.Products.Responses;
using Microsoft.Data.SqlClient;

namespace APIStoreManager.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly StoreManagerContext _db;

        public ProductController(StoreManagerContext db)
        {
            _db = db;
        }
        [HttpGet("MyProductsList/{shopId}")]
        [Authorize]
        public async Task<IActionResult> GetMyShopProducts(long shopId)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

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
        public async Task<IActionResult> CreateProduct(long shopId, [FromBody] ProductDto dto)
        {
            try
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
            catch (DbUpdateException ex) when (IsDuplicateNameError(ex))
            {
                
                return Conflict(new
                {
                    Message = $"Tên sản phẩm '{dto.Name}' đã tồn tại trong hệ thống",
                    ErrorCode = "PRODUCT_NAME_DUPLICATE"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi không xác định", Detail = ex.Message });
            }
        }


        [HttpPut("{productId}/UpdateProduct")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(long productId, [FromBody] ProductDto updatedProduct)
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


        private bool IsDuplicateNameError(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2601 || sqlEx.Number == 2627); 
        }
    }
}