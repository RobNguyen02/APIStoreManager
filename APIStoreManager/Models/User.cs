using System;
using System.Collections.Generic;

namespace APIStoreManager.Models;

public partial class User
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? NickName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
}
