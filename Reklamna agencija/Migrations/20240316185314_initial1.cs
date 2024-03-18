using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reklamna_agencija.Migrations
{
    /// <inheritdoc />
    public partial class initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminAgencijeId",
                table: "ReklamniPanoi",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReklamniPanoi_AdminAgencijeId",
                table: "ReklamniPanoi",
                column: "AdminAgencijeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReklamniPanoi_AdminiAgencije_AdminAgencijeId",
                table: "ReklamniPanoi",
                column: "AdminAgencijeId",
                principalTable: "AdminiAgencije",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReklamniPanoi_AdminiAgencije_AdminAgencijeId",
                table: "ReklamniPanoi");

            migrationBuilder.DropIndex(
                name: "IX_ReklamniPanoi_AdminAgencijeId",
                table: "ReklamniPanoi");

            migrationBuilder.DropColumn(
                name: "AdminAgencijeId",
                table: "ReklamniPanoi");
        }
    }
}
