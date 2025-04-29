using System.ComponentModel.DataAnnotations;

namespace APIStoreManager.Models
{
    public class ProductDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị cần trong khoảng cho phép/" +
            "" +
            ".")]
        public decimal Price { get; set; }

     
        public string Description { get; set; }


    }
}
