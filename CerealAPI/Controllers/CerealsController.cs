using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CerealAPI.Data;
using CerealAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace CerealAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CerealsController : ControllerBase
    {
        private readonly CerealContext _context;

        public CerealsController(CerealContext context)
        {
            _context = context;
        }

        // GET: api/cereals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cereal>>> GetCereals(
            [FromQuery] string? mfr,
            [FromQuery] int? caloriesMin,
            [FromQuery] int? caloriesMax,
            [FromQuery] int? sugarsMin,
            [FromQuery] int? sugarsMax,
            [FromQuery] string? sort)
        {
            IQueryable<Cereal> query = _context.Cereals;

            // Filtrering på producent
            if (!string.IsNullOrEmpty(mfr))
                query = query.Where(c => c.Mfr == mfr);

            // Intervalbaseret filtrering på kalorier
            if (caloriesMin.HasValue)
                query = query.Where(c => c.Calories >= caloriesMin.Value);
            if (caloriesMax.HasValue)
                query = query.Where(c => c.Calories <= caloriesMax.Value);

            // Intervalbaseret filtrering på sukker
            if (sugarsMin.HasValue)
                query = query.Where(c => c.Sugars >= sugarsMin.Value);
            if (sugarsMax.HasValue)
                query = query.Where(c => c.Sugars <= sugarsMax.Value);

            // Sortering
            if (!string.IsNullOrEmpty(sort))
            {
                query = sort.ToLower() switch
                {
                    "calories_asc" => query.OrderBy(c => c.Calories),
                    "calories_desc" => query.OrderByDescending(c => c.Calories),
                    "sugars_asc" => query.OrderBy(c => c.Sugars),
                    "sugars_desc" => query.OrderByDescending(c => c.Sugars),
                    "rating_asc" => query.OrderBy(c => c.Rating),
                    "rating_desc" => query.OrderByDescending(c => c.Rating),
                    _ => query
                };
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }



        // GET: api/cereals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cereal>> GetCereal(int id)
        {
            var cereal = await _context.Cereals.FindAsync(id);
            if (cereal == null)
                return NotFound();

            return cereal;
        }

        // POST: api/cereals
        [HttpPost, Authorize(Roles = "admin")]
        public async Task<ActionResult<Cereal>> PostCereal(Cereal cereal)
        {
            _context.Cereals.Add(cereal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCereal), new { id = cereal.Id }, cereal);
        }

        // PUT: api/cereals/5
        [HttpPut("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutCereal(int id, Cereal cereal)
        {
            if (id != cereal.Id)
                return BadRequest();

            _context.Entry(cereal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cereals.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/cereals/5
        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCereal(int id)
        {
            var cereal = await _context.Cereals.FindAsync(id);
            if (cereal == null)
                return NotFound();

            _context.Cereals.Remove(cereal);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
