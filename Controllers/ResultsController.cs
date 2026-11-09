
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
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ResultsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Organisers see results for their own events.
        // Participants see only their own results.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var query = _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.User)
                .AsQueryable();

            if (User.IsInRole("Organiser"))
            {
                query = query.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.Event != null &&
                    r.Enrolment.Event.OrganiserId == userId
                );
            }
            else if (User.IsInRole("Participant"))
            {
                query = query.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.UserId == userId
                );
            }
            else
            {
                return Forbid();
            }

            return await query.ToListAsync();
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.User)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound();

            if (User.IsInRole("Organiser"))
            {
                if (result.Enrolment?.Event?.OrganiserId != userId)
                    return Forbid();
            }
            else if (User.IsInRole("Participant"))
            {
                if (result.Enrolment?.UserId != userId)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

            return result;
        }

        // POST: api/Results
        // Organiser records a participant's result.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Result>> CreateResult(Result result)
        {
            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == result.EnrolmentId
                );

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.Event?.OrganiserId != userId)
                return Forbid();

            if (result.FinishTime <= TimeSpan.Zero)
                return BadRequest("Please enter a valid finishing time.");

            if (result.Position <= 0)
                return BadRequest("Position must be greater than zero.");

            bool resultExists = await _context.Results.AnyAsync(r =>
                r.EnrolmentId == result.EnrolmentId
            );

            if (resultExists)
                return BadRequest(
                    "A result has already been recorded for this participant."
                );

            result.ResultId = 0;
            result.Enrolment = null;

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result captured successfully!",
                resultId = result.ResultId
            });
        }

        // PUT: api/Results/5
        // Organiser updates a result.
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id,
            Result updatedResult)
        {
            if (id != updatedResult.ResultId)
                return BadRequest("Result ID does not match.");

            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var existingResult = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (existingResult == null)
                return NotFound();

            if (existingResult.Enrolment?.Event?.OrganiserId != userId)
                return Forbid();

            if (updatedResult.FinishTime <= TimeSpan.Zero)
                return BadRequest("Please enter a valid finishing time.");

            if (updatedResult.Position <= 0)
                return BadRequest("Position must be greater than zero.");

            existingResult.FinishTime = updatedResult.FinishTime;
            existingResult.Position = updatedResult.Position;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Results/5
        // Organiser deletes a result.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var userIdString = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound();

            if (result.Enrolment?.Event?.OrganiserId != userId)
                return Forbid();

            _context.Results.Remove(result);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
