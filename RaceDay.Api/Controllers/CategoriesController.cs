
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == id)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.Description
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] RaceDayCategory category)
        {
            // Check whether the user is logged in.
            int? loggedInUserId =
                HttpContext.Session.GetInt32("UserId");

            string? userRole =
                HttpContext.Session.GetString("Role");

            if (loggedInUserId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in before creating a category."
                });
            }

            // Only organisers can create categories.
            if (userRole != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only organisers can create categories."
                });
            }

            // Check that the event exists and belongs to this organiser.
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == category.EventId);

            if (raceEvent == null)
            {
                return BadRequest(new
                {
                    message = "The EventId must belong to an existing event."
                });
            }

            if (raceEvent.OrganiserId != loggedInUserId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only create categories for your own events."
                });
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            category.CategoryId = 0;
            category.Event = null;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                new
                {
                    category.CategoryId,
                    category.EventId,
                    category.Name,
                    category.Description
                });
        }
    }
}
