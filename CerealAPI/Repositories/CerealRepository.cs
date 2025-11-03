using Microsoft.EntityFrameworkCore;
using CerealAPI.Models;
using CerealAPI.Interfaces;
using CerealAPI.Data;

namespace CerealAPI.Repositories
{
    public class CerealRepository : ICerealRepository
    {
        private readonly CerealContext _context;

        public CerealRepository(CerealContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cereal>> GetCerealsAsync(
            string? mfr = null,
            int? caloriesMin = null,
            int? caloriesMax = null,
            int? sugarsMin = null,
            int? sugarsMax = null,
            string? sort = null)

        {
            IQueryable<Cereal> query = _context.Cereals;

            if (!string.IsNullOrEmpty(mfr)) query = query.Where(c => c.Mfr == mfr);
            if (caloriesMin.HasValue) query = query.Where(c => c.Calories >= caloriesMin.Value);
            if (caloriesMax.HasValue) query = query.Where(c => c.Calories <= caloriesMax.Value);
            if (sugarsMin.HasValue) query = query.Where(c => c.Sugars >= sugarsMin.Value);
            if (sugarsMax.HasValue) query = query.Where(c => c.Sugars <= sugarsMax.Value);

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

            return await query.ToListAsync();
        }

        public async Task<Cereal?> GetCerealByIdAsync(int id)
        {
            return await _context.Cereals.FindAsync(id);
        }

        public async Task<Cereal> AddCerealAsync(Cereal cereal)
        {
            _context.Cereals.Add(cereal);
            await _context.SaveChangesAsync();
            return cereal;
        }

        public async Task<bool> UpdateCerealAsync(Cereal cereal)
        {
            _context.Entry(cereal).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cereals.Any(e => e.Id == cereal.Id)) return false;
                throw;
            }
        }

        public async Task<bool> DeleteCerealAsync(int id)
        {
            var cereal = await _context.Cereals.FindAsync(id);
            if (cereal == null) return false;

            _context.Cereals.Remove(cereal);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
