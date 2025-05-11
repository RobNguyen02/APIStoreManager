using APIStoreManager.Data;
using APIStoreManager.DTOs.Shops.Requests;
using APIStoreManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIStoreManager.Controllers
{

    [Route("api/[Controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly StoreManagerContext _db;
        public ShopController(StoreManagerContext db) => _db = db;
        


        [HttpPost("CreateShop")]
        [Authorize]
        public async Task<IActionResult> CreateShop([FromBody] ShopDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

                var shop = new Shop
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description,
                    OwnerId = userId
                };

                _db.Shops.Add(shop);
                await _db.SaveChangesAsync();

                return Ok(shop);
            }
            catch (DbUpdateException ex) when (IsDuplicateNameError(ex))
            {
                return Conflict(new
                {
                    Message = $"Tên cửa hàng '{dto.Name}' đã tồn tại",
                    ErrorCode = "SHOP_NAME_DUPLICATE"
                });
            }
        }


        [HttpGet("MyShopsList")]
        [Authorize]
        public async Task<IActionResult> GetMyShops()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var myShops = await _db.Shops.Where(s => s.OwnerId == userId).ToListAsync();
            return Ok(myShops);
        }


        [HttpPut("{shopId}/updateshop")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> UpdateShop(int shopId, [FromBody] ShopDto updatedShop)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (string.IsNullOrWhiteSpace(updatedShop.Name))
            {
                return BadRequest("Tên shop không được để trống!");
            }
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                //  UPDLOCK
                var shop = await _db.Shops
                    .FromSqlInterpolated($"SELECT * FROM Shops WITH (UPDLOCK) WHERE Id = {shopId} AND OwnerId = {userId}")
                    .FirstOrDefaultAsync();

                if (shop == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Shop không tồn tại hoặc không thuộc quyền sở hữu!");
                }

                shop.Name = updatedShop.Name;
                shop.Description = updatedShop.Description;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Cập nhật shop thành công!",
                    data = new { shop.Id, shop.Name, shop.Description }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }

        }


        [HttpDelete("{shopId}/deleteshop")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> DeleteShop(int shopId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var shop = await _db.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == shopId && s.OwnerId == userId);

            if (shop == null)
                return NotFound("Shop không tồn tại hoặc không thuộc quyền sở hữu!");

            _db.Products.RemoveRange(shop.Products);
            _db.Shops.Remove(shop);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Đã xóa shop và sản phẩm.");
        }
        private bool IsDuplicateNameError(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

    }
}
