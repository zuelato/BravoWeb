using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BravoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateDataToContentFragment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "ContentFragments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "ContentFragments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentFragments_TemplateId",
                table: "ContentFragments",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentFragments_CustomTemplates_TemplateId",
                table: "ContentFragments",
                column: "TemplateId",
                principalTable: "CustomTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentFragments_CustomTemplates_TemplateId",
                table: "ContentFragments");

            migrationBuilder.DropIndex(
                name: "IX_ContentFragments_TemplateId",
                table: "ContentFragments");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "ContentFragments");

            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "ContentFragments");
        }
    }
}
