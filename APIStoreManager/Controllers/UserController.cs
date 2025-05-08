using APIStoreManager.Data;
using APIStoreManager.DTOs.Users.Requests;
using APIStoreManager.DTOs.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly StoreManagerContext _db;
        public UserController(StoreManagerContext db) => _db = db;



        [HttpGet("Profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> GetUserProfile()
        {
            var username = User.Identity?.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized("Không tìm thấy User");

            return new UserResponseDto
            {
            
                NickName = user.NickName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl
            };
        }

        [HttpPut("UpdateInfo")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserDto dto)
        {
            var username = User.Identity?.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized("Không tìm thấy User");

            if (!string.IsNullOrEmpty(dto.NickName))
            {
                if (await _db.Users.AnyAsync(u => u.NickName == dto.NickName && u.Username != username))
                    return BadRequest("Nickname đã được sử dụng bởi người dùng khác");

                user.NickName = dto.NickName;
            }

            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;

            await _db.SaveChangesAsync();
            return Ok("Thông tin người dùng đã được cập nhật");
        }


        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var username = User.Identity?.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Mật khẩu hiện tại không đúng");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();

            return Ok("Đổi mật khẩu thành công");
        }


     
    }
}
