using CerealAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<CerealContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Controllers
builder.Services.AddControllers();

// SeedService
builder.Services.AddTransient<SeedService>();

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
});

var app = builder.Build();

// Kør SeedService for at loade Cereal.csv
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seed = services.GetRequiredService<SeedService>();
    seed.SeedCereals();
}

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cereal API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
