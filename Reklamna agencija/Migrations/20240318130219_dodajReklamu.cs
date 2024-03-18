using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reklamna_agencija.Migrations
{
    /// <inheritdoc />
    public partial class dodajReklamu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrojDana",
                table: "Reklame");

            migrationBuilder.DropColumn(
                name: "Cijena",
                table: "Reklame");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Reklame",
                type: "bit",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reklame",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrojDana",
                table: "Reklame",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Cijena",
                table: "Reklame",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
