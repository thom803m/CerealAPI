using Microsoft.EntityFrameworkCore;
using CerealAPI.Models;

namespace CerealAPI.Data
{
    // DbContext for CerealAPI (håndterer databasen)
    public class CerealContext : DbContext
    {
        public CerealContext(DbContextOptions<CerealContext> options) : base(options) { }

        // DbSet for Cereal (repræsenterer Cereals-tabellen i databasen)
        public DbSet<Cereal> Cereals { get; set; }

        // DbSet for User (repræsenterer Users-tabellen i databasen)
        public DbSet<User> Users { get; set; }

        // Konfiguration af databasen
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Sørger for, at produktnavne er unikke
            modelBuilder.Entity<Cereal>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Sørger for, at brugernavne er unikke
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
