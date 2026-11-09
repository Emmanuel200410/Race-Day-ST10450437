
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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CategoriesController(ApplicationDBContext context)
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

        // GET: api/categories
        // Organisers see categories for their own events.
        // Participants can view all categories.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var query = _context.Categories
                .AsNoTracking()
                .Include(c => c.Event)
                .AsQueryable();

            if (User.IsInRole("Organiser"))
            {
                int? organiserId = GetCurrentUserId();

                if (organiserId == null)
                    return Unauthorized();

                query = query.Where(c =>
                    c.Event != null &&
                    c.Event.OrganiserId == organiserId.Value
                );
            }

            return await query.ToListAsync();
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            if (User.IsInRole("Organiser"))
            {
                int? organiserId = GetCurrentUserId();

                if (organiserId == null)
                    return Unauthorized();

                if (category.Event?.OrganiserId != organiserId.Value)
                    return Forbid();
            }

            return category;
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Category>> CreateCategory(
            Category category)
        {
            int? organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            bool eventBelongsToOrganiser = await _context.Events
                .AnyAsync(e =>
                    e.EventId == category.EventId &&
                    e.OrganiserId == organiserId.Value
                );

            if (!eventBelongsToOrganiser)
            {
                return BadRequest(
                    "You can only add categories to your own events."
                );
            }

            category.CategoryId = 0;
            category.Event = null;

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                category
            );
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            Category category)
        {
            if (id != category.CategoryId)
                return BadRequest("Category ID does not match.");

            int? organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            var existingCategory = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (existingCategory == null)
                return NotFound();

            if (existingCategory.Event?.OrganiserId != organiserId.Value)
                return Forbid();

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.EntryFee = category.EntryFee;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int? organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            var category = await _context.Categories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            if (category.Event?.OrganiserId != organiserId.Value)
                return Forbid();

            if (category.Enrolments.Any())
            {
                return BadRequest(
                    "This category cannot be deleted because participants are enrolled."
                );
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }
}
