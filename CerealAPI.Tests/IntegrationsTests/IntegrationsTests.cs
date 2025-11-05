using CerealAPI.Controllers;
using CerealAPI.Data;
using CerealAPI.DTOs;
using CerealAPI.Models;
using CerealAPI.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CerealAPI.Tests.IntegrationsTests
{
    public class IntegrationTests
    {
        private async Task<(AuthController authController, CerealsController cerealsController)> CreateControllersAsync()
        {
            var options = new DbContextOptionsBuilder<CerealContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationTestDb_AllInOne")
                .Options;

            var context = new CerealContext(options);

            // Ryd databasen
            context.Cereals.RemoveRange(context.Cereals);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            // Seed cerealer
            context.Cereals.AddRange(
                new Cereal { Name = "Corn Flakes", Mfr = "K", Type = "C", Calories = 100, Protein = 3, Fat = 1, Sodium = 200, Sugars = 5, Rating = 50 },
                new Cereal { Name = "Froot Loops", Mfr = "K", Type = "C", Calories = 120, Protein = 2, Fat = 1, Sodium = 180, Sugars = 12, Rating = 70 },
                new Cereal { Name = "Special K", Mfr = "K", Type = "C", Calories = 90, Protein = 2, Fat = 1, Sodium = 150, Sugars = 3, Rating = 80 }
            );

            // Seed brugere
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                Role = "admin"
            });
            context.Users.Add(new User
            {
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("abcd"),
                Role = "user"
            });

            await context.SaveChangesAsync();

            var userRepo = new UserRepository(context);
            var cerealRepo = new CerealRepository(context);

            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "EnLangSikkerSecretKey"},
                {"Jwt:Issuer", "CerealApi"},
                {"Jwt:Audience", "CerealApiUsers"},
            };
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var authController = new AuthController(configuration, userRepo);
            var cerealsController = new CerealsController(cerealRepo);

            return (authController, cerealsController);
        }

        [Fact]
        public async Task AuthController_Login_ShouldReturnToken_And_CerealsController_ShouldReturnCereals()
        {
            var (authController, cerealsController) = await CreateControllersAsync();

            // Login som admin
            var loginResponse = await authController.Login(new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            });
            var okLogin = Assert.IsType<OkObjectResult>(loginResponse);
            var loginResult = Assert.IsType<LoginResponse>(okLogin.Value);
            loginResult.Token.Should().NotBeNullOrEmpty();

            // Simuler HttpContext (valgfrit)
            cerealsController.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            cerealsController.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Bearer {loginResult.Token}";

            // Hent cerealer
            var cerealsResponse = await cerealsController.GetCereals(null, null, null, null, null, null);
            var okCereals = Assert.IsType<OkObjectResult>(cerealsResponse.Result);
            var cereals = Assert.IsAssignableFrom<IEnumerable<Cereal>>(okCereals.Value);
            cereals.Should().HaveCount(3);
        }

        [Fact]
        public async Task Admin_CanAddAndDeleteCereal()
        {
            var (authController, cerealsController) = await CreateControllersAsync();

            // Login som admin
            var loginResponse = await authController.Login(new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            });
            var token = ((LoginResponse)((OkObjectResult)loginResponse).Value).Token;

            cerealsController.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            cerealsController.ControllerContext.HttpContext.Request.Headers["Authorization"] = $"Bearer {token}";

            // Tilføj ny cereal
            var newCereal = new Cereal { Name = "Cheerios", Mfr = "G", Type = "C", Calories = 110, Protein = 3, Fat = 1, Sodium = 190, Sugars = 6, Rating = 60 };
            var addResult = await cerealsController.PostCereal(newCereal);
            var createdResult = Assert.IsType<CreatedAtActionResult>(addResult.Result);
            var addedCereal = Assert.IsType<Cereal>(createdResult.Value);
            addedCereal.Name.Should().Be("Cheerios");

            // Slet cerealen
            var deleteResult = await cerealsController.DeleteCereal(addedCereal.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
        {
            var (authController, _) = await CreateControllersAsync();

            var result = await authController.Login(new LoginRequest
            {
                Username = "admin",
                Password = "forkert"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetCereals_WithFilterAndSort_ShouldReturnExpectedResults()
        {
            var (_, cerealsController) = await CreateControllersAsync();

            // Filter: Calories >= 100, Sort by Rating descending
            var result = await cerealsController.GetCereals(
                mfr: "K",
                caloriesMin: 100,
                caloriesMax: null,
                sugarsMin: null,
                sugarsMax: null,
                sort: "rating_desc"
            );

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cereals = Assert.IsAssignableFrom<IEnumerable<Cereal>>(okResult.Value);

            cereals.Should().HaveCount(2); // Corn Flakes og Froot Loops
            cereals.First().Name.Should().Be("Froot Loops"); // Rating 70
            cereals.Last().Name.Should().Be("Corn Flakes");  // Rating 50
        }

        [Fact]
        public async Task GetCereals_WithSugarsFilter_ShouldReturnExpectedResults()
        {
            var (_, cerealsController) = await CreateControllersAsync();

            // Filter: Sugars <= 5
            var result = await cerealsController.GetCereals(
                mfr: null,
                caloriesMin: null,
                caloriesMax: null,
                sugarsMin: null,
                sugarsMax: 5,
                sort: null
            );

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cereals = Assert.IsAssignableFrom<IEnumerable<Cereal>>(okResult.Value);

            cereals.Should().HaveCount(2); // Corn Flakes og Special K
            cereals.Select(c => c.Name).Should().Contain(new[] { "Corn Flakes", "Special K" });
        }
    }
}
