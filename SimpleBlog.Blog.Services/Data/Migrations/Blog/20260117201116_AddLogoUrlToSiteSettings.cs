using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBlog.Blog.Services.Data.Migrations.Blog
{
    /// <inheritdoc />
    public partial class AddLogoUrlToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "SiteSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "SiteSettings");
        }
    }
}
