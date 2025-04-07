using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pedido.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTentativasEnvio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TentativasEnvio",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TentativasEnvio",
                table: "Pedidos");
        }
    }
}
