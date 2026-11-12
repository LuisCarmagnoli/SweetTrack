using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SweetTrack.Ordering.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialOrderingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaRuta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransportistaId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NombreChofer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PatenteVehiculo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoRuta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPedido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KioskUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RazonSocialKiosco = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CuitKiosco = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DireccionEntrega = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoIva = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstadoActual = table.Column<int>(type: "int", nullable: false),
                    MetodoPago = table.Column<int>(type: "int", nullable: false),
                    NotasEntrega = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryRouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_DeliveryRoutes_DeliveryRouteId",
                        column: x => x.DeliveryRouteId,
                        principalTable: "DeliveryRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoBarrasSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreProductoSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CantidadUnidades = table.Column<int>(type: "int", nullable: false),
                    CantidadBultos = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitarioSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubtotalLinea = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "int", nullable: false),
                    EstadoNuevo = table.Column<int>(type: "int", nullable: false),
                    UsuarioModificadorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotasMotivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryRouteId",
                table: "Orders",
                column: "DeliveryRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_NumeroPedido",
                table: "Orders",
                column: "NumeroPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistories_OrderId",
                table: "StatusHistories",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "StatusHistories");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryRoutes");
        }
    }
}
