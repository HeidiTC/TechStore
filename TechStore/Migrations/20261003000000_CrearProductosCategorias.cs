using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace TechStore.Migrations
{
    public partial class CrearProductosCategorias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Categorias", x => x.ID); });
            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CategoriaID = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.ID);
                    table.ForeignKey("FK_Productos_Categorias_CategoriaID", x => x.CategoriaID,
                        "Categorias", "ID", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex("IX_Productos_CategoriaID", "Productos", "CategoriaID");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Productos");
            migrationBuilder.DropTable("Categorias");
        }
    }
}
