using CerealAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using CerealAPI.DTOs;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    // Midlertidig bruger-liste (i praksis bør dette komme fra DB), men lige nu er der kun en bruger: admin
    private static List<User> users = new List<User>
    {
        new User { Id = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("T1h2o3m4a5s6+"), Role = "admin" }
    };

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest login)
    {
        var user = users.SingleOrDefault(u => u.Username == login.Username); // Find bruger baseret på brugernavn
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            return Unauthorized("Forkert brugernavn eller password"); // Returnerer 401 hvis brugernavn eller password er forkert

        // Opret JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]); // Hent nøglen fra konfiguration
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7), // Token udløber om 7 dage
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor); // Opret token

        return Ok(new LoginResponse // Returner token og udløbstid
        {
            Token = tokenHandler.WriteToken(token), // Token til brug i Swagger
            Expires = tokenDescriptor.Expires.Value
        });
    }
}
