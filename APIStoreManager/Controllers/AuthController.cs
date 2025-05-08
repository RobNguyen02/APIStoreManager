using APIStoreManager.Data;
using APIStoreManager.Models;
using APIStoreManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace APIStoreManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly StoreManagerContext _db;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        public AuthController(StoreManagerContext db, TokenService tokenService, IConfiguration config)
        {
            _db = db;
            _tokenService = tokenService;
            _config = config;
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
            var refreshToken = _tokenService.GenerateRefreshToken();

            
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();

            return Ok(new TokenResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = DateTime.UtcNow.Add(TimeSpan.Parse(_config["Jwt:AccessTokenExpiry"]))
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequest tokenRequest)
        {
            if (tokenRequest == null || string.IsNullOrEmpty(tokenRequest.AccessToken) || string.IsNullOrEmpty(tokenRequest.RefreshToken))
                return BadRequest("Invalid client request");

            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
            var username = principal.Identity.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.RefreshToken != tokenRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest("Invalid refresh token");

            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // new refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = DateTime.UtcNow.Add(TimeSpan.Parse(_config["Jwt:AccessTokenExpiry"]))
            });
        }
    }
      

    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresIn { get; set; }
    }
}
