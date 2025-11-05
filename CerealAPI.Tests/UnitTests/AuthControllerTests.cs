using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using CerealAPI.Controllers;
using CerealAPI.Interfaces;
using CerealAPI.Models;
using CerealAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BCrypt.Net;

namespace CerealAPI.Tests
{
    public class AuthControllerTests
    {
        private AuthController CreateController(User? user = null)
        {
            // Mock IUserRepository
            var mockRepo = new Mock<IUserRepository>();
            if (user != null)
            {
                mockRepo.Setup(r => r.GetUserByUsernameAsync(user.Username))
                        .ReturnsAsync(user);
            }
            else
            {
                mockRepo.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>()))
                        .ReturnsAsync((User?)null);
            }

            // In-memory IConfiguration med JWT settings
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "EnLangSikkerSecretKeySomDuIkkeCommiter"},
                {"Jwt:Issuer", "CerealApi"},
                {"Jwt:Audience", "CerealApiUsers"},
                {"Jwt:ExpireDays", "7"}
            };
            var configuration = new ConfigurationBuilder()
                                .AddInMemoryCollection(inMemorySettings)
                                .Build();

            return new AuthController(configuration, mockRepo.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            var controller = CreateController();

            var result = await controller.Login(new LoginRequest { Username = "unknown", Password = "1234" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIncorrect()
        {
            var user = new User
            {
                Username = "test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("rigtigtpassword"),
                Role = "admin"
            };

            var controller = CreateController(user);

            var result = await controller.Login(new LoginRequest { Username = "test", Password = "forkert" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken_WhenCredentialsValid()
        {
            var user = new User
            {
                Username = "test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                Role = "admin"
            };

            var controller = CreateController(user);

            var result = await controller.Login(new LoginRequest { Username = "test", Password = "1234" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);

            Assert.NotNull(response.Token);
            Assert.True(response.Expires > System.DateTime.UtcNow);
        }
    }
}
