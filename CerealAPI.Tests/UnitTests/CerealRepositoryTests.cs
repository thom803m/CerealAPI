using Xunit;
using Microsoft.EntityFrameworkCore;
using CerealAPI.Data;
using CerealAPI.Repositories;
using CerealAPI.Models;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;

namespace CerealAPI.Tests
{
    public class CerealRepositoryTests
    {
        // Helper-funktion til at oprette en test-cereal med default værdier
        private Cereal CreateTestCereal(string name)
        {
            return new Cereal
            {
                Name = name,
                Mfr = "K",
                Type = "C",
                Calories = 100,
                Protein = 3,
                Fat = 1,
                Sodium = 200,
                Fiber = 1.0f,
                Carbo = 20.0f,
                Sugars = 5,
                Potass = 50,
                Vitamins = 25,
                Shelf = 1,
                Weight = 1.0f,
                Cups = 1.0f,
                Rating = 50
            };
        }

        // Opretter repository med InMemory database, unik per test
        private async Task<CerealRepository> GetRepositoryAsync()
        {
            var options = new DbContextOptionsBuilder<CerealContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unik db per test
                .Options;

            var context = new CerealContext(options);

            context.Cereals.AddRange(
                CreateTestCereal("Corn Flakes"),
                CreateTestCereal("Froot Loops")
            );

            await context.SaveChangesAsync();

            return new CerealRepository(context);
        }

        [Fact]
        public async Task GetCerealsAsync_ShouldReturnAll_WhenNoFilter()
        {
            var repo = await GetRepositoryAsync();

            var result = await repo.GetCerealsAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCerealByIdAsync_ShouldReturnCorrectCereal()
        {
            var repo = await GetRepositoryAsync();

            var allCereals = (await repo.GetCerealsAsync()).ToList();
            var id = allCereals.First().Id;

            var result = await repo.GetCerealByIdAsync(id);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Corn Flakes");
        }

        [Fact]
        public async Task AddCerealAsync_ShouldIncreaseCount()
        {
            var repo = await GetRepositoryAsync();

            var newCereal = new Cereal
            {
                Name = "Special K",
                Mfr = "K",
                Type = "C",
                Calories = 90,
                Protein = 2,
                Fat = 1,
                Sodium = 150,
                Fiber = 1.0f,
                Carbo = 18.0f,
                Sugars = 3,
                Potass = 40,
                Vitamins = 25,
                Shelf = 1,
                Weight = 1.0f,
                Cups = 1.0f,
                Rating = 80
            };

            await repo.AddCerealAsync(newCereal);

            var all = await repo.GetCerealsAsync();
            all.Should().HaveCount(3);
            all.Should().Contain(c => c.Name == "Special K");
        }

        [Fact]
        public async Task UpdateCerealAsync_ShouldModifyExistingCereal()
        {
            var repo = await GetRepositoryAsync();

            var cereal = (await repo.GetCerealsAsync()).First();
            cereal.Calories = 200;

            var result = await repo.UpdateCerealAsync(cereal);
            result.Should().BeTrue();

            var updated = await repo.GetCerealByIdAsync(cereal.Id);
            updated!.Calories.Should().Be(200);
        }

        [Fact]
        public async Task DeleteCerealAsync_ShouldRemoveCereal()
        {
            var repo = await GetRepositoryAsync();

            var cereal = (await repo.GetCerealsAsync()).First();
            var result = await repo.DeleteCerealAsync(cereal.Id);
            result.Should().BeTrue();

            var all = await repo.GetCerealsAsync();
            all.Should().HaveCount(1);
            all.Should().NotContain(c => c.Id == cereal.Id);
        }
    }
}
