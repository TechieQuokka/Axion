using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERP.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("mock-login")]
        [AllowAnonymous]
        public IActionResult MockLogin([FromBody] MockLoginRequest request)
        {
            // 개발용 Mock 로그인 - 실제 환경에서는 실제 인증 로직 구현 필요
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required");
            }

            var token = GenerateJwtToken(request.Email, request.CompanyId ?? 1);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = 1,
                    email = request.Email,
                    name = "Test User",
                    companyId = request.CompanyId ?? 1,
                    department = "Development",
                    position = "Developer"
                },
                expiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult RefreshToken()
        {
            // 리프레시 토큰 구현 (향후)
            return Ok(new { message = "Refresh token not implemented yet" });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // 로그아웃 로직 (향후 - 토큰 블랙리스트 등)
            return Ok(new { message = "Logged out successfully" });
        }

        private string GenerateJwtToken(string email, int companyId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-must-be-at-least-32-characters-long-for-security";
            var issuer = jwtSettings["Issuer"] ?? "ERP-System";
            var audience = jwtSettings["Audience"] ?? "erp-api";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, email),
                new Claim("CompanyId", companyId.ToString()),
                new Claim("BusinessUserId", "1"),
                new Claim("Department", "Development"),
                new Claim("Position", "Developer")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class MockLoginRequest
    {
        public string Email { get; set; } = "test@test.com";
        public int? CompanyId { get; set; } = 1;
    }
}
