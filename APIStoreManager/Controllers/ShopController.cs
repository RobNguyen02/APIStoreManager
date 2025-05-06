using APIStoreManager.Data;
using APIStoreManager.DTOs.Shops.Responses;
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
        private readonly StoreManagerContext _db;
        public ShopController(StoreManagerContext db) => _db = db;
        
        [HttpPost("CreateShop")]
        [Authorize]
        public async Task<IActionResult> CreateShop([FromBody] ShopDto dto)
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


        [HttpPut("{shopId}/updateshop")]
        [Authorize]
        public async Task<IActionResult> UpdateShop(int shopId, [FromBody] ShopDto updatedShop)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == shopId && s.OwnerId == userId);
            if (shop == null) return NotFound("Shop không tồn tại hoặc không thuộc quyền sở hữu!");

            shop.Name = updatedShop.Name;
            shop.Description = updatedShop.Description;
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật shop thành công!",
                data = shop
            });
        }


        [HttpDelete("{shopId}/deleteshop")]
        [Authorize]
        public async Task<IActionResult> DeleteShop(int shopId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var shop = await _db.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == shopId && s.OwnerId == userId);
            if (shop == null) return NotFound("Shop không tồn tại hoặc không thuộc quyền sở hữu!");

            _db.Products.RemoveRange(shop.Products);
            _db.Shops.Remove(shop);
            await _db.SaveChangesAsync();
            return Ok("Đã xóa shop.");
        }
    }
}