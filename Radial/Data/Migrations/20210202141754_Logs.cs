using Microsoft.EntityFrameworkCore.Migrations;

namespace Radial.Data.Migrations
{
    public partial class Logs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentEnergy",
                table: "Users",
                newName: "IsServerAdmin");

            migrationBuilder.AddColumn<long>(
                name: "ChargeCurrent",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EnergyCurrent",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    ID = table.Column<string>(type: "TEXT", nullable: false),
                    LogLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    TimeStamp = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropColumn(
                name: "ChargeCurrent",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EnergyCurrent",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsServerAdmin",
                table: "Users",
                newName: "CurrentEnergy");
        }
    }
}
