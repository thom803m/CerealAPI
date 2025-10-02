using Microsoft.EntityFrameworkCore;
using CerealAPI.Models;

namespace CerealAPI.Data
{
    public class CerealContext : DbContext
    {
        public CerealContext(DbContextOptions<CerealContext> options) : base(options) { }

        public DbSet<Cereal> Cereals { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cereal>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
