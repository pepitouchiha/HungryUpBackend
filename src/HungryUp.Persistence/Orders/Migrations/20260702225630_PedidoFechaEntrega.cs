using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HungryUp.Persistence.Orders.Migrations
{
    /// <inheritdoc />
    public partial class PedidoFechaEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "Pedidos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "Pedidos");
        }
    }
}
