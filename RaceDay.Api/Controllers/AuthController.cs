
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public AuthController(RaceDayContext context)
        {
            _context = context;
        }

        public class RegisterRequest
        {
            [Required, EmailAddress, MaxLength(255)]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(8)]
            public string Password { get; set; } = string.Empty;

            [Required, MaxLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required, MaxLength(100)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            public string Role { get; set; } = "Participant";
        }

        public class LoginRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public class UserResponse
        {
            public int UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private static UserResponse ToResponse(RaceDayUser user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();
            string role = request.Role.Trim();

            if (role != "Organiser" && role != "Participant")
            {
                return BadRequest(new
                {
                    message = "Role must be Organiser or Participant."
                });
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "An account with this email already exists."
                });
            }

            var user = new RaceDayUser
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    request.Password),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, ToResponse(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(
                    request.Password, user.PasswordHash))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);

            return Ok(ToResponse(user));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return Unauthorized(new
                {
                    message = "You are not logged in."
                });
            }

            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in first."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();

                return Unauthorized(new
                {
                    message = "User account was not found."
                });
            }

            return Ok(ToResponse(user));
        }
    }
}
