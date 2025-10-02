using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CerealAPI.Data;
using CerealAPI.Models;

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
        public async Task<ActionResult<IEnumerable<Cereal>>> GetCereals()
        {
            return await _context.Cereals.ToListAsync();
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
        [HttpPost]
        public async Task<ActionResult<Cereal>> PostCereal(Cereal cereal)
        {
            _context.Cereals.Add(cereal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCereal), new { id = cereal.Id }, cereal);
        }

        // PUT: api/cereals/5
        [HttpPut("{id}")]
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
        [HttpDelete("{id}")]
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
