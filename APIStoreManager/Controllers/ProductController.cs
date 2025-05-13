using APIStoreManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using APIStoreManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using APIStoreManager.DTOs.Products.Requests;
using APIStoreManager.DTOs.Products.Responses;

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


        [HttpGet("{shopId}/Products")]
        [Authorize]
        public async Task<IActionResult> GetPublicProductsByShop(long shopId)
        {
            var shop = await _db.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == shopId);

            if (shop == null)
                return NotFound("Không tìm thấy cửa hàng.");

            var products = shop.Products.Select(p => new
            {
                productId = p.Id,
                productName = p.Name,
                price = p.Price,
                description = p.Description
            }).ToList();

            return Ok(new
            {
                shopId = shop.Id,
                shopName = shop.Name,
                shopDescription = shop.Description,
                products = products
            });
        }

        [HttpGet("All")]
        [Authorize]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _db.Products
                .Include(p => p.Shop)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    ShopId = p.ShopId
                })
                .ToListAsync();

            return Ok(products);
        }
        [HttpGet("{productId}/product-details/")]
        [Authorize]
        public async Task<IActionResult> GetProductById(long productId)
        {
            var product = await _db.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound("Không tìm thấy sản phẩm.");
            }

            var productDto = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ShopId = product.ShopId
            };

            return Ok(productDto);
        }


        [HttpPost("{shopId}/CreateProduct")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> CreateProduct(long shopId, [FromBody] ProductDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (dto.Price <= 0)
                    return BadRequest("Giá sản phẩm phải lớn hơn 0");
                //
                var shop = await _db.Shops
                    .FromSqlInterpolated($"SELECT * FROM Shops WITH (UPDLOCK) WHERE Id = {shopId} AND OwnerId = {userId}")
                    .FirstOrDefaultAsync();

                if (shop == null)
                    return NotFound("Cửa hàng không tồn tại hoặc không thuộc quyền sở hữu!");

                if (await _db.Products.AnyAsync(p => p.ShopId == shopId && p.Name == dto.Name))
                {
                    return Conflict(new
                    {
                        Message = $"Tên sản phẩm '{dto.Name}' đã tồn tại trong cửa hàng",
                        ErrorCode = "PRODUCT_NAME_DUPLICATE"
                    });
                }

                var product = new Product
                {
                    Name = dto.Name.Trim(),
                    Price = dto.Price,
                    Description = dto.Description?.Trim(),
                    ShopId = shopId
                };

                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();


                return Ok(new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    ShopId = product.ShopId
                });
            }
            catch (DbUpdateException ex) when (IsDuplicateNameError(ex))
            {
                await transaction.RollbackAsync();
                return Conflict(new
                {
                    Message = $"Tên sản phẩm '{dto.Name}' đã tồn tại trong hệ thống",
                    ErrorCode = "PRODUCT_NAME_DUPLICATE"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
            return Ok(new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ShopId = product.ShopId
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