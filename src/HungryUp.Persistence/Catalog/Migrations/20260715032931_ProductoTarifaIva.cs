using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HungryUp.Persistence.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class ProductoTarifaIva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TarifaIva",
                table: "Productos",
                type: "TEXT",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 19m); // IVA general en Colombia; aplica a productos existentes al migrar
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TarifaIva",
                table: "Productos");
        }
    }
}
