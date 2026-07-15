using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HungryUp.Persistence.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class ProductoCostoPromedio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoPromedio",
                table: "Productos",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoPromedio",
                table: "Productos");
        }
    }
}
