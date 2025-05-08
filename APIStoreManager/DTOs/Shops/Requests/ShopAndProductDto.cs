using APIStoreManager.DTOs.Products.Requests;

namespace APIStoreManager.DTOs.Shops.Requests
{
    public class ShopAndProductsDto
    {
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }
}
