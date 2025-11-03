using CerealAPI.Models;

namespace CerealAPI.Interfaces
{
    public interface ICerealRepository
    {
        Task<IEnumerable<Cereal>> GetCerealsAsync(
            string? mfr = null,
            int? caloriesMin = null,
            int? caloriesMax = null,
            int? sugarsMin = null,
            int? sugarsMax = null,
            string? sort = null);

        Task<Cereal?> GetCerealByIdAsync(int id);
        Task<Cereal> AddCerealAsync(Cereal cereal);
        Task<bool> UpdateCerealAsync(Cereal cereal);
        Task<bool> DeleteCerealAsync(int id);
    }
}
