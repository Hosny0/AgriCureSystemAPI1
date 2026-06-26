using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriCureSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class hhhhhhhhh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DiseaseScan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symptoms",
                table: "DiseaseScan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Treatment",
                table: "DiseaseScan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "DiseaseScan");

            migrationBuilder.DropColumn(
                name: "Symptoms",
                table: "DiseaseScan");

            migrationBuilder.DropColumn(
                name: "Treatment",
                table: "DiseaseScan");
        }
    }
}
