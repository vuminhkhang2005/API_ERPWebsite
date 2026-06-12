using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using WebERP.Data;
using WebERP.Helpers;

namespace WebERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DbHelper db;
        private readonly IConfiguration config;

        public AuthController(DbHelper db, IConfiguration config)
        {
            this.db = db;
            this.config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { success = false, message = "Username and Password are required." });
            }

            UserRecord user = null;

            using (var conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT UserId, Username, PasswordHash, Role, CustomerId FROM Users WHERE Username = @Username";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", dto.Username.Trim());
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new UserRecord
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = reader["Role"].ToString(),
                                CustomerId = reader["CustomerId"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["CustomerId"])
                            };
                        }
                    }
                }
            }

            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { success = false, message = "Invalid username or password." });
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };

            if (user.CustomerId.HasValue)
            {
                claims.Add(new Claim("CustomerId", user.CustomerId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Append httpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Force secure since we are on HTTPS
                SameSite = SameSiteMode.None, // Required for cross-origin local requests
                Expires = DateTime.UtcNow.AddHours(2)
            };

            Response.Cookies.Append("jwt_token", tokenString, cookieOptions);

            return Ok(new
            {
                success = true,
                username = user.Username,
                role = user.Role,
                customerId = user.CustomerId
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { success = true, message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            int? customerId = string.IsNullOrEmpty(customerIdClaim) ? null : int.Parse(customerIdClaim);

            return Ok(new
            {
                success = true,
                username,
                role,
                customerId
            });
        }
    }

    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserRecord
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public int? CustomerId { get; set; }
    }
}
