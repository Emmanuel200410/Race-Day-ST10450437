
using Api_proj.Data;
using Api_proj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api_proj.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EnrolmentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/enrolments
        // Organisers see enrolments for their own events.
        // Participants see only their own enrolments.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var query = _context.Enrolments
                .AsNoTracking()
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.User)
                .AsQueryable();

            if (User.IsInRole("Organiser"))
            {
                query = query.Where(e =>
                    e.Event != null &&
                    e.Event.OrganiserId == userId
                );
            }
            else if (User.IsInRole("Participant"))
            {
                query = query.Where(e => e.UserId == userId);
            }
            else
            {
                return Forbid();
            }

            return await query.ToListAsync();
        }

        // POST: api/enrolments
        // Participants enrol in an event using a selected category.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(
            Enrolment enrolment)
        {
            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            // Check that the event exists.
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == enrolment.EventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Check that the category exists and belongs to this event.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == enrolment.CategoryId &&
                    c.EventId == enrolment.EventId
                );

            if (category == null)
            {
                return BadRequest(
                    "The selected category does not belong to this event."
                );
            }

            // Prevent the same participant from enrolling twice.
            bool alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
                e.UserId == userId &&
                e.EventId == enrolment.EventId
            );

            if (alreadyEnrolled)
            {
                return BadRequest(
                    "You are already enrolled in this event."
                );
            }

            // Assign the logged-in participant.
            enrolment.EnrolmentId = 0;
            enrolment.UserId = userId;
            enrolment.EnrolmentDate = DateTime.UtcNow;
            enrolment.Status = "Confirmed";

            enrolment.Event = null;
            enrolment.Category = null;
            enrolment.User = null;

            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Successfully enrolled in the event!",
                enrolmentId = enrolment.EnrolmentId
            });
        }
    }
}
