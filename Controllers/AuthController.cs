using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.DTOs;
using EmployeeScheduling.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeScheduling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IJwtService jwtService, IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                {
                    return Ok(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Ok(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var expirationHours = int.Parse(_configuration.GetSection("JwtSettings")["ExpirationHours"]);

                var response = new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new LoginResponse
                    {
                        Token = token,
                        User = new UserInfo
                        {
                            UserId = user.UserId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Role = user.Role
                        },
                        ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Get current user information (requires authentication)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserInfo>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .Where(u => u.UserId == userId && u.IsActive)
                    .Select(u => new UserInfo
                    {
                        UserId = u.UserId,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user information" });
            }
        }

        /// <summary>
        /// Validate token (useful for checking if token is still valid)
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            return Ok(new { message = "Token is valid", isValid = true });
        }

        /// <summary>
        /// Logout (client-side token removal, server acknowledges)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // In a JWT system, logout is typically handled client-side by removing the token
            // This endpoint confirms the logout action
            return Ok(new { message = "Logout successful" });
        }
    }
}
