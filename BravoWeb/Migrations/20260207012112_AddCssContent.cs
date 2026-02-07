using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BravoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddCssContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CssContent",
                table: "ContentFragments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CssContent",
                table: "ContentFragments");
        }
    }
}
