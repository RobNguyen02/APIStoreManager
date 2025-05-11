namespace APIStoreManager.DTOs.Products.Responses
{
    public class ProductResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public long ShopId { get; set; } 
    }
}