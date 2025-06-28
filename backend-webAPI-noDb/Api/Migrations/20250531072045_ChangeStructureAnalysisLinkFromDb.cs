using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStructureAnalysisLinkFromDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Link",
                table: "AnalysisLinks",
                newName: "ResultData");

            migrationBuilder.AddColumn<string>(
                name: "LinkOrKeyword",
                table: "AnalysisLinks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "AnalysisLinks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkOrKeyword",
                table: "AnalysisLinks");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "AnalysisLinks");

            migrationBuilder.RenameColumn(
                name: "ResultData",
                table: "AnalysisLinks",
                newName: "Link");
        }
    }
}
