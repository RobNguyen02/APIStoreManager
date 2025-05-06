using System;
using System.Collections.Generic;

namespace APIStoreManager.Models;

public partial class Product
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public double Price { get; set; }

    public string? Description { get; set; }

    public long ShopId { get; set; }

    public virtual Shop Shop { get; set; } = null!;
}
