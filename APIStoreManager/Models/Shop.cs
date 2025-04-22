using APIStoreManager.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIStoreManager.Models
{
    public class Shop
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public int OwnerId { get; set; }
        
        public User Owner { get; set; } = null!;
    
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
