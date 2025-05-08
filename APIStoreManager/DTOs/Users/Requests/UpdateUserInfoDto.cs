using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.DTOs.Users.Requests
{
    public class UpdateUserDto
    {
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
