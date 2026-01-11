using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBlog.Blog.Services.Data.Migrations.Blog
{
    /// <inheritdoc />
    public partial class AddIsPinnedToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Posts");
        }
    }
}
