using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BravoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddSitePages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PageId",
                table: "ContentFragments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SitePages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentFragments_PageId",
                table: "ContentFragments",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePages_Slug",
                table: "SitePages",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentFragments_SitePages_PageId",
                table: "ContentFragments",
                column: "PageId",
                principalTable: "SitePages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentFragments_SitePages_PageId",
                table: "ContentFragments");

            migrationBuilder.DropTable(
                name: "SitePages");

            migrationBuilder.DropIndex(
                name: "IX_ContentFragments_PageId",
                table: "ContentFragments");

            migrationBuilder.DropColumn(
                name: "PageId",
                table: "ContentFragments");
        }
    }
}
