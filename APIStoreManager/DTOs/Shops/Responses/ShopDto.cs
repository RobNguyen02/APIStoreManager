using System.ComponentModel.DataAnnotations;
namespace APIStoreManager.DTOs.Shops.Responses
{
    public class ShopDto
    {
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
