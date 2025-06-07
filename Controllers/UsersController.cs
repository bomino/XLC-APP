using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EmployeeScheduling.API.Data;
using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ RE-ENABLE AUTHENTICATION
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users (Admin and Manager only)
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5 (Admin, Manager, or own record)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            // Get current user ID from token
            var currentUserIdClaim = User.FindFirst("userId")?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            // Allow if user is Admin/Manager or requesting their own record
            if (currentUserRole != "Admin" && currentUserRole != "Manager" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _context.Users
                .Where(u => u.UserId == id && u.IsActive)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST: api/users (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateUser(CreateUserRequest request)
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? "Employee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.IsActive,
                user.CreatedAt
            });
        }
    }

    // DTO for creating users
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}
