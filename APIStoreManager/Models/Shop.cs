using System;
using System.Collections.Generic;

namespace APIStoreManager.Models;

public partial class Shop
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public long OwnerId { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
