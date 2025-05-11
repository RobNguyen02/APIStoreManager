using APIStoreManager.Data;
using APIStoreManager.Models;
using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.Services
{
    public class ShopService
    {
        private readonly StoreManagerContext _db;

        public ShopService(StoreManagerContext db)
        {
            _db = db;
        }

        public async Task<List<Shop>> GetMyShopsAsync(int userId)
        {
            return await _db.Shops.Where(s => s.OwnerId == userId).ToListAsync();
        }
    }
}
