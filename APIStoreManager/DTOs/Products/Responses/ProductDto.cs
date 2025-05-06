using System.ComponentModel.DataAnnotations;

namespace APIStoreManager.DTOs.Products.Responses
{
    public class ProductDto
    {

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không vượt quá 200 ký tự")]
        public string Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm không hợp lệ")]
        public double Price { get; set; }  

        [StringLength(1000, ErrorMessage = "Mô tả không vượt quá 1000 ký tự")]
        public string Description { get; set; }

    }
}