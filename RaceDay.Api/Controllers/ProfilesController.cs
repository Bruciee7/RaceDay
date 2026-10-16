
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/profiles")]
    public class ProfilesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ProfilesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/profiles/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in to view your profile."
                });
            }

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId.Value)
                .Select(u => new
                {
                    u.UserId,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Role,
                    u.ProfilePictureUrl,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            return Ok(user);
        }

        // PUT: api/profiles/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateProfileRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in to update your profile."
                });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new
                {
                    message = "First name and last name are required."
                });
            }

            if (request.FirstName.Length > 100 ||
                request.LastName.Length > 100)
            {
                return BadRequest(new
                {
                    message = "First name and last name cannot exceed 100 characters."
                });
            }

            if (request.ProfilePictureUrl != null &&
                request.ProfilePictureUrl.Length > 500)
            {
                return BadRequest(new
                {
                    message = "Profile picture URL cannot exceed 500 characters."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.ProfilePictureUrl = request.ProfilePictureUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role,
                user.ProfilePictureUrl,
                user.CreatedAt
            });
        }
    }

    public class UpdateProfileRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }
    }
}
