using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBlog.Blog.Services.Data.Migrations.Blog
{
    /// <inheritdoc />
    public partial class AddAboutImageAndContactText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "AboutMe",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactText",
                table: "SiteSettings",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AboutMe");

            migrationBuilder.DropColumn(
                name: "ContactText",
                table: "SiteSettings");
        }
    }
}
