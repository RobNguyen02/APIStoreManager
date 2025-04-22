using System.Text.Json.Serialization;

namespace APIStoreManager.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;

        public int ShopId { get; set; }
        [JsonIgnore]
        public Shop Shop { get; set; } = null!;
    }
}
