using CsvHelper;
using CsvHelper.Configuration;
using CerealAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CerealAPI.Interfaces;

namespace CerealAPI.Data
{
    public class SeedService
    {
        private readonly CerealContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IUserRepository _userRepo;

        public SeedService(CerealContext context, IWebHostEnvironment env, IUserRepository userRepo)
        {
            _context = context;
            _env = env;
            _userRepo = userRepo;
        }

        public async Task SeedCerealsAsync()
        {
            if (await _context.Cereals.AnyAsync())
                return;

            var filePath = Path.Combine(_env.ContentRootPath, "Data", "SeedData", "Cereal.csv");
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV filen blev ikke fundet: {filePath}");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null
            });

            csv.Context.RegisterClassMap<CerealMap>();
            var cereals = csv.GetRecords<Cereal>().ToList();

            await _context.Cereals.AddRangeAsync(cereals);
            await _context.SaveChangesAsync();
        }

        public async Task SeedAdminUserAsync()
        {
            // Spring over, hvis admin allerede findes
            var existingAdmin = await _userRepo.GetUserByUsernameAsync("admin");
            if (existingAdmin != null) return;

            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("T1h2o3m4a5s6+"),
                Role = "admin"
            };

            await _userRepo.AddUserAsync(adminUser);
        }
    }

    // CSV mapping som før
    public sealed class CerealMap : ClassMap<Cereal>
    {
        public CerealMap()
        {
            Map(m => m.Name).Name("name");
            Map(m => m.Mfr).Name("mfr");
            Map(m => m.Type).Name("type");
            Map(m => m.Calories).Name("calories");
            Map(m => m.Protein).Name("protein");
            Map(m => m.Fat).Name("fat");
            Map(m => m.Sodium).Name("sodium");
            Map(m => m.Fiber).Name("fiber");
            Map(m => m.Carbo).Name("carbo");
            Map(m => m.Sugars).Name("sugars");
            Map(m => m.Potass).Name("potass");
            Map(m => m.Vitamins).Name("vitamins");
            Map(m => m.Shelf).Name("shelf");
            Map(m => m.Weight).Name("weight");
            Map(m => m.Cups).Name("cups");
            Map(m => m.Rating).Name("rating");
        }
    }
}