using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HungryUp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPurchasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumeroFactura = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    NombreProveedor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NitProveedor = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    ReteFuentePorc = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ReteIvaPorc = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ReteIcaPorMil = table.Column<decimal>(type: "TEXT", precision: 6, scale: 3, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineasCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompraId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoNombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TarifaIva = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasCompra_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineasCompra_CompraId",
                table: "LineasCompra",
                column: "CompraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineasCompra");

            migrationBuilder.DropTable(
                name: "Compras");
        }
    }
}
