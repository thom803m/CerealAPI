using CsvHelper;
using CsvHelper.Configuration;
using CerealAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CerealAPI.Data
{
    public class SeedService
    {
        private readonly CerealContext _context;
        private readonly IWebHostEnvironment _env;

        public SeedService(CerealContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task SeedCerealsAsync()
        {
            // Spring hvis der allerede er data
            if (await _context.Cereals.AnyAsync())
                return;

            var filePath = Path.Combine(_env.ContentRootPath, "Data", "SeedData", "Cereal.csv");
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV filen blev ikke fundet: {filePath}");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",          // Semikolon separator
                HeaderValidated = null,
                MissingFieldFound = null
            });

            // Map CSV-kolonner til Cereal properties
            csv.Context.RegisterClassMap<CerealMap>();

            var cereals = csv.GetRecords<Cereal>().ToList();

            await _context.Cereals.AddRangeAsync(cereals);
            await _context.SaveChangesAsync();
        }
    }

    // Mapping mellem CSV og Cereal properties
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
            // ImagePath er nullable, så vi springer den over
        }
    }
}
