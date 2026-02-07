using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BravoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddJsContentToContentFragment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsContent",
                table: "ContentFragments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsContent",
                table: "ContentFragments");
        }
    }
}
