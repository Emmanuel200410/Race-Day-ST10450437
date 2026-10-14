using Api_proj.Data;
using Api_proj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_proj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteInformationController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public RouteInformationController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/routeinformation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteInformation>>> GetRoutes()
        {
            return await _context.RouteInformation.ToListAsync();
        }

        // GET: api/routeinformation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteInformation>> GetRoute(int id)
        {
            var route = await _context.RouteInformation.FindAsync(id);

            if (route == null)
            {
                return NotFound();
            }

            return route;
        }

        // POST: api/routeinformation
        [HttpPost]
        public async Task<ActionResult<RouteInformation>> CreateRoute(
            RouteInformation route)
        {
            _context.RouteInformation.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRoute),
                new { id = route.RouteId },
                route);
        }

        // PUT: api/routeinformation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(
            int id,
            RouteInformation route)
        {
            if (id != route.RouteId)
            {
                return BadRequest();
            }

            _context.Entry(route).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/routeinformation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.RouteInformation.FindAsync(id);

            if (route == null)
            {
                return NotFound();
            }

            _context.RouteInformation.Remove(route);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}