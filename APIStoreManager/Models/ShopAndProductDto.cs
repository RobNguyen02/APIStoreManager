namespace APIStoreManager.Models
{
    public class ShopAndProductsDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }
}
