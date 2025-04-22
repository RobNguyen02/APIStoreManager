using System.ComponentModel.DataAnnotations;

namespace APIStoreManager.Models
{
    public class CreateShopDto
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
