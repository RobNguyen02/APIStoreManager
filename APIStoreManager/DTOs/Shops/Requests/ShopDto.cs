using System.ComponentModel.DataAnnotations;
namespace APIStoreManager.DTOs.Shops.Requests;

public class ShopDto
{
    public string Name { get; set; }

    public string? Description { get; set; }
}
