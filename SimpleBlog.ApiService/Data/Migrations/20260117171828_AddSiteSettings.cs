using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBlog.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration was superseded by a subsequent migration that includes all changes.
            // Rolling back is not supported as it would require complex data migration logic.
            throw new NotSupportedException("This migration cannot be reversed. Please use a fresh database or restore from backup.");
        }
    }
}
