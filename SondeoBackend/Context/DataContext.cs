using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.CustomIdentity;
using SondeoBackend.DTO;
using SondeoBackend.Models;
using System.Reflection.Emit;

namespace SondeoBackend.Context
{
    public class DataContext : IdentityDbContext<CustomUser,CustomRole,int>
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Canal> Canales { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<DetalleEncuesta> DetalleEncuestas { get; set; }
        public DbSet<Encuesta> Encuestas { get; set; }
        public DbSet<Local> Locales { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Propiedades> Propiedades { get; set; }
        public DbSet<Medicion> Mediciones { get; set; }
        public DbSet<Producto> Productos { get; set; }
    }
}
