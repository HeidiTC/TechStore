using TechStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace TechStore.Migrations
{
    [DbContext(typeof(TechStoreContext))]
    [Migration("20261003000000_CrearProductosCategorias")]
    partial class CrearProductosCategorias
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);
            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
            modelBuilder.Entity("TechStore.Models.Categoria", b =>
            {
                b.Property<int>("ID").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));
                b.Property<string>("Nombre").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("Descripcion").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.HasKey("ID");
                b.ToTable("Categorias");
            });
            modelBuilder.Entity("TechStore.Models.Producto", b =>
            {
                b.Property<int>("ID").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));
                b.Property<string>("Nombre").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("Descripcion").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<decimal>("Precio").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
                b.Property<int>("CategoriaID").HasColumnType("int");
                b.Property<int>("Stock").HasColumnType("int");
                b.Property<bool>("Estado").HasColumnType("bit");
                b.HasKey("ID");
                b.HasIndex("CategoriaID");
                b.ToTable("Productos");
            });
            modelBuilder.Entity("TechStore.Models.Producto", b =>
            {
                b.HasOne("TechStore.Models.Categoria", "Categoria")
                    .WithMany("Productos").HasForeignKey("CategoriaID")
                    .OnDelete(DeleteBehavior.Restrict).IsRequired();
                b.Navigation("Categoria");
            });
            modelBuilder.Entity("TechStore.Models.Categoria", b => { b.Navigation("Productos"); });
        }
    }
}
