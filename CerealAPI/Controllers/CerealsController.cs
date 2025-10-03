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

        // GET alle produkter med optional filtrering og sortering
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

            // Filtrering
            if (!string.IsNullOrEmpty(mfr)) 
                query = query.Where(c => c.Mfr == mfr); // Filtrering på producent
            if (caloriesMin.HasValue) 
                query = query.Where(c => c.Calories >= caloriesMin.Value); // Filtrering på minimum kalorier, når der er angivet værdi
            if (caloriesMax.HasValue)
                query = query.Where(c => c.Calories <= caloriesMax.Value); // Filtrering på maksimum kalorier, når der er angivet værdi
            if (sugarsMin.HasValue)
                query = query.Where(c => c.Sugars >= sugarsMin.Value); // Filtrering på minimum sukker, når der er angivet værdi
            if (sugarsMax.HasValue)
                query = query.Where(c => c.Sugars <= sugarsMax.Value); // Filtrering på maksimum sukker, når der er angivet værdi

            // Sortering
            if (!string.IsNullOrEmpty(sort))
            {
                query = sort.ToLower() switch
                {
                    "calories_asc" => query.OrderBy(c => c.Calories), // Stigende kalorier skriv "calories_asc"
                    "calories_desc" => query.OrderByDescending(c => c.Calories), // Faldende kalorier skriv "calories_desc"
                    "sugars_asc" => query.OrderBy(c => c.Sugars), // Stigende sukker skriv "sugars_asc"
                    "sugars_desc" => query.OrderByDescending(c => c.Sugars), // Faldende sukker skriv "sugars_desc"
                    "rating_asc" => query.OrderBy(c => c.Rating), // Stigende rating skriv "rating_asc"
                    "rating_desc" => query.OrderByDescending(c => c.Rating), // Faldende rating skriv "rating_desc"
                    _ => query
                };
            }

            var result = await query.ToListAsync();
            return Ok(result); // Returnerer listen over produkter
        }

        // GET enkeltprodukt efter ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Cereal>> GetCereal(int id)
        {
            var cereal = await _context.Cereals.FindAsync(id); // Finder produktet efter ID
            if (cereal == null)
                return NotFound(); // Returnerer 404 hvis produktet ikke findes

            return cereal; // Returnerer produktet
        }

        // POST nyt produkt (kræver adminrolle)
        [HttpPost, Authorize(Roles = "admin")]
        public async Task<ActionResult<Cereal>> PostCereal(Cereal cereal)
        {
            _context.Cereals.Add(cereal); // Tilføjer det nye produkt til konteksten
            await _context.SaveChangesAsync(); // Gemmer ændringerne i databasen

            return CreatedAtAction(nameof(GetCereal), new { id = cereal.Id }, cereal); // Returnerer 201 Created med lokation til det nye produkt
        }

        // PUT opdatering (kræver adminrolle)
        [HttpPut("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutCereal(int id, Cereal cereal)
        {
            if (id != cereal.Id) // ID i URL skal matche ID i kroppen
                return BadRequest(); // Returnerer 400 Bad Request hvis de ikke matcher

            _context.Entry(cereal).State = EntityState.Modified; // Markerer entiteten som modificeret

            try
            {
                await _context.SaveChangesAsync(); // Gemmer ændringerne i databasen
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cereals.Any(e => e.Id == id))
                    return NotFound(); // Returnerer 404 hvis produktet ikke findes
                else
                    throw;
            }

            return NoContent(); // Returnerer 204 No Content ved succesfuld opdatering
        }

        // DELETE (kræver adminrolle)
        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCereal(int id)
        {
            var cereal = await _context.Cereals.FindAsync(id);
            if (cereal == null)
                return NotFound(); // Returnerer 404 hvis produktet ikke findes

            _context.Cereals.Remove(cereal); // Fjerner produktet fra konteksten
            await _context.SaveChangesAsync(); // Gemmer ændringerne i databasen

            return NoContent(); // Returnerer 204 No Content ved succesfuld sletning
        }
    }
}
