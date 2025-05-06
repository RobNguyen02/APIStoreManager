using APIStoreManager.Data;
using APIStoreManager.Models;
using APIStoreManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIStoreManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly StoreManagerContext _db;
        private readonly TokenService _tokenService;

        public AuthController(StoreManagerContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (await _db.Users.AnyAsync(u => u.Username == username))
                return BadRequest("Tên người dùng đã được sử dụng!");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Username = username,
                PasswordHash = hashedPassword
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("Đăng ký thành công!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Unauthorized("Đăng nhập thất bại, kiểm tra lại username hoặc password!");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Mật khẩu không hợp lệ!");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }
    }
}
