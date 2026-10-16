
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api/enrolments")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/enrolments
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    e.CategoryId,
                    e.Status,
                    e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentId == id)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    e.CategoryId,
                    e.Status,
                    e.EnrolmentDate
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            return Ok(enrolment);
        }

        // POST: api/enrolments
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] RaceDayEnrolment enrolment)
        {
            // Check that the user is logged in.
            int? loggedInUserId =
                HttpContext.Session.GetInt32("UserId");

            string? userRole =
                HttpContext.Session.GetString("Role");

            if (loggedInUserId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in before enrolling in an event."
                });
            }

            // Only participants can enrol.
            if (userRole != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only participants can enrol in events."
                });
            }

            // Participants can only enrol themselves.
            if (enrolment.ParticipantId != loggedInUserId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only create an enrolment for your own account."
                });
            }

            var participantExists = await _context.Users
                .AnyAsync(u =>
                    u.UserId == loggedInUserId.Value &&
                    u.Role == "Participant");

            if (!participantExists)
            {
                return BadRequest(new
                {
                    message = "The participant account could not be found."
                });
            }

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == enrolment.EventId);

            if (!eventExists)
            {
                return BadRequest(new
                {
                    message = "EventId must belong to an existing event."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == enrolment.CategoryId &&
                    c.EventId == enrolment.EventId);

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "CategoryId must belong to the selected event."
                });
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == loggedInUserId.Value &&
                    e.EventId == enrolment.EventId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "This participant is already enrolled in this event."
                });
            }

            enrolment.EnrolmentId = 0;
            enrolment.ParticipantId = loggedInUserId.Value;
            enrolment.Status = "Pending";
            enrolment.EnrolmentDate = DateTime.Now;
            enrolment.Participant = null;
            enrolment.Event = null;
            enrolment.Category = null;
            enrolment.Result = null;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                new
                {
                    enrolment.EnrolmentId,
                    enrolment.ParticipantId,
                    enrolment.EventId,
                    enrolment.CategoryId,
                    enrolment.Status,
                    enrolment.EnrolmentDate
                });
        }
    }
}

