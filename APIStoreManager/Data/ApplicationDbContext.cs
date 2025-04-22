using Microsoft.EntityFrameworkCore;
using APIStoreManager.Model;
using APIStoreManager.Models;

namespace ApiStoreManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<Product> Products => Set<Product>();
    }
}

