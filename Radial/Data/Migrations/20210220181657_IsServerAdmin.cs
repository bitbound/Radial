using Microsoft.EntityFrameworkCore.Migrations;

namespace Radial.Data.Migrations
{
    public partial class IsServerAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargeCurrent",
                table: "CharacterInfo");

            migrationBuilder.DropColumn(
                name: "IsServerAdmin",
                table: "CharacterInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsServerAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsServerAdmin",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "ChargeCurrent",
                table: "CharacterInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsServerAdmin",
                table: "CharacterInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
