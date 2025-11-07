using CerealAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CerealAPI.Swagger;
using CerealAPI.Interfaces;
using CerealAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<CerealContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
    options => options.EnableRetryOnFailure()));

// Register repository
builder.Services.AddScoped<ICerealRepository, CerealRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Controllers
builder.Services.AddControllers();

// SeedService
builder.Services.AddTransient<SeedService>();

// JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Cereal API",
        Description = "API til håndtering af morgenmadsprodukter"
    });

    // JWT authorization i Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Indsæt 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });

    // Tilføj operation filter for rolle-beskrivelser
    c.OperationFilter<CerealAPI.Swagger.AuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// Kør SeedService ved opstart med retry, hvis MySQL ikke er klar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<CerealContext>();

    // Retry loop for MySQL
    bool dbReady = false;
    int retries = 0;
    while (!dbReady && retries < 10)
    {
        try
        {
            if (db.Database.CanConnect())
            {
                dbReady = true;
            }
        }
        catch
        {
            retries++;
            Console.WriteLine("MySQL ikke klar, venter 3 sekunder...");
            await Task.Delay(3000);
        }
    }

    var seed = services.GetRequiredService<SeedService>();
    await seed.SeedCerealsAsync();
    await seed.SeedAdminUserAsync();
}

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cereal API v1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();