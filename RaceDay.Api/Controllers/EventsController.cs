
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RaceDayEvent>>> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RaceDayEvent>> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(raceEvent);
        }

        // POST: api/events
        [HttpPost]
        public async Task<ActionResult<RaceDayEvent>> CreateEvent(
            [FromBody] RaceDayEvent raceEvent)
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
                    message = "Please log in before creating an event."
                });
            }

            // Only organisers can create events.
            if (userRole != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only organisers can create events."
                });
            }

            // The event must belong to the logged-in organiser.
            if (raceEvent.OrganiserId != loggedInUserId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only create events under your own organiser account."
                });
            }

            // Confirm that the organiser still exists.
            bool organiserExists = await _context.Users
                .AnyAsync(u =>
                    u.UserId == loggedInUserId.Value &&
                    u.Role == "Organiser");

            if (!organiserExists)
            {
                return BadRequest(new
                {
                    message = "The organiser account could not be found."
                });
            }

            raceEvent.CreatedAt = DateTime.Now;
            raceEvent.UpdatedAt = null;

            _context.Events.Add(raceEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventId },
                raceEvent);
        }
    }
}
