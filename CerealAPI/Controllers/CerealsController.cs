using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CerealAPI.Models;
using CerealAPI.Interfaces;

namespace CerealAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CerealsController : ControllerBase
    {
        private readonly ICerealRepository _repo;

        public CerealsController(ICerealRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cereal>>> GetCereals(
            [FromQuery] string? mfr,
            [FromQuery] int? caloriesMin,
            [FromQuery] int? caloriesMax,
            [FromQuery] int? sugarsMin,
            [FromQuery] int? sugarsMax,
            [FromQuery] string? sort)
        {
            var cereals = await _repo.GetCerealsAsync(mfr, caloriesMin, caloriesMax, sugarsMin, sugarsMax, sort);
            return Ok(cereals);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cereal>> GetCereal(int id)
        {
            var cereal = await _repo.GetCerealByIdAsync(id);
            if (cereal == null) return NotFound();
            return cereal;
        }

        [HttpPost, Authorize(Roles = "admin")]
        public async Task<ActionResult<Cereal>> PostCereal(Cereal cereal)
        {
            var created = await _repo.AddCerealAsync(cereal);
            return CreatedAtAction(nameof(GetCereal), new { id = created.Id }, created);
        }

        [HttpPut("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutCereal(int id, Cereal cereal)
        {
            if (id != cereal.Id) return BadRequest();
            var updated = await _repo.UpdateCerealAsync(cereal);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCereal(int id)
        {
            var deleted = await _repo.DeleteCerealAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}