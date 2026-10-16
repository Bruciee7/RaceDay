
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [Route("api/results")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/results
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .AsNoTracking()
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.FinishTime,
                    r.FinishPosition
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/results/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .AsNoTracking()
                .Where(r => r.ResultId == id)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.FinishTime,
                    r.FinishPosition
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            return Ok(result);
        }

        // POST: api/results
        [HttpPost]
        public async Task<IActionResult> CreateResult(
            [FromBody] RaceDayResult result)
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
                    message = "Please log in before adding a result."
                });
            }

            // Only organisers can add results.
            if (userRole != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only organisers can add race results."
                });
            }

            if (string.IsNullOrWhiteSpace(result.FinishTime) ||
                result.FinishTime.Length > 20)
            {
                return BadRequest(new
                {
                    message = "FinishTime is required and must not exceed 20 characters."
                });
            }

            if (result.FinishPosition <= 0)
            {
                return BadRequest(new
                {
                    message = "FinishPosition must be greater than zero."
                });
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(
                    e => e.EnrolmentId == result.EnrolmentId);

            if (enrolment == null)
            {
                return BadRequest(new
                {
                    message = "EnrolmentId must belong to an existing enrolment."
                });
            }

            // Confirm that the event belongs to the logged-in organiser.
            if (enrolment.Event == null ||
                enrolment.Event.OrganiserId != loggedInUserId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only add results for your own events."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentId == result.EnrolmentId);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            result.ResultId = 0;
            result.Enrolment = null;

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultId },
                new
                {
                    result.ResultId,
                    result.EnrolmentId,
                    result.FinishTime,
                    result.FinishPosition
                });
        }
    }
}
