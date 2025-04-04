using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pedido.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoCampoDeDatasParaRastreamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Pedidos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEnvio",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DataEnvio",
                table: "Pedidos");
        }
    }
}
