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

        // GET: api/enrolments (Organisers view all; or participants view their own)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userRole == "Organiser")
            {
                // Organisers can view all enrolments
                return await _context.Enrolments
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .ToListAsync();
            }
            else
            {
                // Participants can only view their own enrolments
                if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

                return await _context.Enrolments
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .ToListAsync();
            }
        }

        // POST: api/enrolments (Participants enter an event by selecting a category)
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(Enrolment enrolment)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Automatically assign the logged-in user's ID
            enrolment.UserId = userId;
            enrolment.EnrolmentDate = DateTime.UtcNow;
            enrolment.Status = "Confirmed";

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully enrolled in the event!", enrolmentId = enrolment.EnrolmentId });
        }
    }
}