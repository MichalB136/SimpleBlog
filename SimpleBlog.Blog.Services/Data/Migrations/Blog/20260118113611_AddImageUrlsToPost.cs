using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBlog.Blog.Services.Data.Migrations.Blog
{
    /// <inheritdoc />
    public partial class AddImageUrlsToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Posts",
                type: "text",
                nullable: true);
        }
    }
}
