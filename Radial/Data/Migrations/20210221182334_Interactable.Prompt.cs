using Microsoft.EntityFrameworkCore.Migrations;

namespace Radial.Data.Migrations
{
    public partial class InteractablePrompt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LookSummary",
                table: "Interactable",
                newName: "Prompt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Prompt",
                table: "Interactable",
                newName: "LookSummary");
        }
    }
}
