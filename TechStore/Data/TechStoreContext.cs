using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;

namespace TechStore.Data
{
    // Unidad 3: el contexto ahora hereda de IdentityDbContext
    // para que en la misma base de datos se guarden los usuarios y los roles (tablas AspNet...)
    public class TechStoreContext : IdentityDbContext<IdentityUser>
    {
        public TechStoreContext(DbContextOptions<TechStoreContext> options) : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Primero se configura Identity (tablas de usuarios y roles)
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
