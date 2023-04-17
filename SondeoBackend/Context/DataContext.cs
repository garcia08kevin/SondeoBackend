using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.DTO;
using SondeoBackend.Models;
using System.Reflection.Emit;

namespace SondeoBackend.Context
{
    public class DataContext : IdentityDbContext<CustomUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {

            if (!options.IsConfigured)

            {

                options.UseSqlServer("A FALLBACK CONNECTION STRING");
            }

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<CustomUser> CustomUsers { get; set; }
    }
}
