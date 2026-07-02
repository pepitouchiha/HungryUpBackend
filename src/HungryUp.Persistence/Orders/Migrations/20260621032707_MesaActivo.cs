using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HungryUp.Persistence.Orders.Migrations
{
    /// <inheritdoc />
    public partial class MesaActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Mesas",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Mesas");
        }
    }
}
