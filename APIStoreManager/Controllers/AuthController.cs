
using ApiStoreManager.Data;
using APIStoreManager.Model;
using APIStoreManager.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace APIStoreManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly TokenService _tokenService;

        public AuthController(ApplicationDbContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
                return BadRequest("Mật khẩu không trùng khớp, nhập lại!");

            if (await _db.Users.AnyAsync(u => u.Username == username))
                return BadRequest("Tên người dùng đã được sử dụng!");

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                PasswordSalt = hmac.Key
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("Đăng ký thành công!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized("Đăng nhập thất bại, kiểm tra lại username hoặc password!");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            if (!computedHash.SequenceEqual(user.PasswordHash)) return Unauthorized("Mật khẩu không hợp lệ!");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });

        }
    }
}