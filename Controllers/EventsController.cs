
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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EventsController(ApplicationDBContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (int.TryParse(userId, out int id))
                return id;

            return null;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Organiser)
                .AsQueryable();

            if (User.IsInRole("Organiser"))
            {
                int? organiserId = GetCurrentUserId();

                if (organiserId == null)
                    return Unauthorized();

                query = query.Where(e =>
                    e.OrganiserId == organiserId.Value
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound();

            return eventItem;
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Event>> CreateEvent(Event eventItem)
        {
            int? organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            bool organiserExists = await _context.Users
                .AnyAsync(u => u.UserId == organiserId.Value);

            if (!organiserExists)
                return BadRequest("Organiser account does not exist.");

            eventItem.EventId = 0;
            eventItem.OrganiserId = organiserId.Value;
            eventItem.Organiser = null;
            eventItem.CreatedDate = DateTime.Now;

            _context.Events.Add(eventItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventId },
                eventItem
            );
        }

        // PUT: api/events/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            Event eventItem)
        {
            if (id != eventItem.EventId)
                return BadRequest("Event ID does not match.");

            int? organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
                return NotFound();

            if (existingEvent.OrganiserId != organiserId.Value)
                return Forbid();

            existingEvent.EventName = eventItem.EventName;
            existingEvent.Description = eventItem.Description;
            existingEvent.EventDate = eventItem.EventDate;
            existingEvent.Location = eventItem.Location;
            existingEvent.Distance = eventItem.Distance;
            existingEvent.EventType = eventItem.EventType;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            var eventItem = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != organiserId.Value)
                return Forbid();

            // Prevent deletion when the event has existing enrolments.
            if (eventItem.Enrolments.Any())
            {
                return BadRequest(
                    "This event cannot be deleted because participants are enrolled."
                );
            }

            _context.Categories.RemoveRange(eventItem.Categories);

            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }
}
